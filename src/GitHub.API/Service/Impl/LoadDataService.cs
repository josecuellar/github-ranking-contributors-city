using GitHub.API.Model;
using GitHub.API.Repository;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GitHub.API.Service.Impl
{
    public class LoadDataService : ILoadDataService
    {

        private IGitHubApiProvider _provider;
        private IStatusRepository _statusService;
        private IRepository _loadDataRepository;

        private const int GITHUB_LIMIT_ROWS_PAGE = 100;
        private const int GITHUB_LIMIT_SAME_QUERY_TOTAL = 1000;
        private const int MONTHS_TO_SEARCH_FOR_RANGE_DATE = 3;

        public LoadDataService(
            IGitHubApiProvider provider,
            IStatusRepository statusService,
            IRepository loadDataRepository)
        {
            _provider = provider;
            _loadDataRepository = loadDataRepository;
            _statusService = statusService;
        }

        public async Task LoadUsersFromLocationByMonthInvertals(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            //TO-DO: Calculate datetime since for intervals ordering first request results by joined user
            DateTime dtStart = new DateTime(2008, 4, 1); 
            //**
            DateTime dtEnd = dtStart.AddMonths(MONTHS_TO_SEARCH_FOR_RANGE_DATE);

            do
            {
                try
                {
                    var dateRange = new DateRange(dtStart, dtEnd);

                    var result = await _provider.GetUsersFrom(location, dateRange, 1, GITHUB_LIMIT_ROWS_PAGE);
                    SetResults(result.Items, location);

                    await PaginateIfNeeded(result.TotalCount, location, dateRange);
                }
                catch (Exception err)
                {
                    Debug.WriteLine(err.Message);
                }
                finally
                {
                    dtStart = dtStart.AddMonths(MONTHS_TO_SEARCH_FOR_RANGE_DATE);
                    dtEnd = dtEnd.AddMonths(MONTHS_TO_SEARCH_FOR_RANGE_DATE);
                }
            }
            while (dtStart <= DateTime.Now);
        }

        public async Task LoadUsersFromLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            var result = await _provider.GetUsersFrom(location, 1, GITHUB_LIMIT_ROWS_PAGE);

            _statusService.SetRunning(location, result.TotalCount);

            SetResults(result.Items, location);

            new Thread(async () =>
            {
                if (CanUseTheSameQueryForAllResults(result.TotalCount))
                    await PaginateIfNeeded(result.TotalCount, location);
                else
                    await LoadUsersFromLocationByMonthInvertals(location);

                await CalculateOrderAndFinalize(result, location);

            }).Start();
        }

        private async Task CalculateOrderAndFinalize(SearchUsersResult result, string location)
        {
            _statusService.SetCalculatingOrder(location);

            await SetAllReposAndCommits(location);

            _statusService.SetFinished(location, result.TotalCount);
        }

        private async Task SetAllReposAndCommits(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            foreach (string user in _loadDataRepository.Get(location).Keys)
            {
                var totalCommits = await _provider.GetTotalCommitsByUser(user);
                var totalRepos = await _provider.GetTotalRepositoriesByUser(user);

                _loadDataRepository.SetReposAndCommitsToUser(location, user, totalCommits, totalRepos);
                _statusService.SetOrderCalculated(location);
            }
        }

        private int GetNumPages(double total)
        {
            return (int)Math.Ceiling((double)total / GITHUB_LIMIT_ROWS_PAGE);
        }

        private bool CanUseTheSameQueryForAllResults(int total)
        {
            return (total <= GITHUB_LIMIT_SAME_QUERY_TOTAL);
        }

        private bool HaveMoreThanOnePage(int total)
        {
            return (total > GITHUB_LIMIT_ROWS_PAGE);
        }

        private void SetResults(IReadOnlyList<User> users, string location)
        {
            var concurrentDict = RankingUser.BuildConcurrentDictionaryFrom(users);
            _statusService.AddLoaded(location, _loadDataRepository.Set(concurrentDict, location));
        }

        private async Task PaginateIfNeeded(int totalCount, string location, DateRange dateRange)
        {
            if (HaveMoreThanOnePage(totalCount))
            {
                int pages = GetNumPages(totalCount);
                for (int i = 2; i <= pages; i++)
                {
                    var resultWithPages = await _provider.GetUsersFrom(location, dateRange, i, GITHUB_LIMIT_ROWS_PAGE);
                    SetResults(resultWithPages.Items, location);
                }
            }
        }

        private async Task PaginateIfNeeded(int totalCount, string location)
        {
            if (HaveMoreThanOnePage(totalCount))
            {
                int pages = GetNumPages(totalCount);
                for (int i = 2; i <= pages; i++)
                {
                    var resultWithPages = await _provider.GetUsersFrom(location, i, GITHUB_LIMIT_ROWS_PAGE);
                    SetResults(resultWithPages.Items, location);
                }
            }
        }
    }
}
