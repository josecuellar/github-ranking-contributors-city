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

            DateTime dtStart = new DateTime(2008, 4, 1); //Github startup started
            DateTime dtEnd = dtStart.AddMonths(MONTHS_TO_SEARCH_FOR_RANGE_DATE);

            do
            {
                try
                {
                    var dateRange = new DateRange(dtStart, dtEnd);

                    var result = await _provider.GetUsersFrom(location, dateRange, 1, GITHUB_LIMIT_ROWS_PAGE);

                    var dataToSave = new List<User>(result.Items);

                    if (HaveMoreThanOnePage(result.TotalCount))
                    {
                        for (int i = 2; i <= GetNumPages(result.TotalCount); i++)
                        {
                            var resultWithPages = await _provider.GetUsersFrom(location, dateRange, i, GITHUB_LIMIT_ROWS_PAGE);

                            dataToSave.AddRange(resultWithPages.Items);
                        }
                    }

                    var concurrentDict = RankingUser.BuildConcurrentDictionaryFrom(dataToSave);
                    _loadDataRepository.Set(concurrentDict, location);
                    _statusService.AddLoaded(location, concurrentDict.Count);

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

            if (CanUseTheSameQueryForAllResults(result.TotalCount))
            {
                var dataToSave = new List<User>(result.Items);

                if (HaveMoreThanOnePage(result.TotalCount))
                {
                    for (int i = 2; i <= GetNumPages(result.TotalCount); i++)
                    {
                        var resultWithPages = await _provider.GetUsersFrom(location, i, GITHUB_LIMIT_ROWS_PAGE);
                        dataToSave.AddRange(resultWithPages.Items);
                    }
                }

                var concurrentDict = RankingUser.BuildConcurrentDictionaryFrom(dataToSave);
                _loadDataRepository.Set(concurrentDict, location);
                _statusService.AddLoaded(location, concurrentDict.Count);

                _statusService.SetCalculatingOrder(location);

                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    await SetAllReposAndCommits(location);

                    _statusService.SetFinished(location, result.TotalCount);

                }).Start();

                return;
            }

            new Thread(async () =>
            {

                Thread.CurrentThread.IsBackground = true;

                await LoadUsersFromLocationByMonthInvertals(location);

                _statusService.SetCalculatingOrder(location);

                await SetAllReposAndCommits(location);

                _statusService.SetFinished(location, result.TotalCount);

            }).Start();
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
    }
}
