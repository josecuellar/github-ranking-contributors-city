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

        private IGitHubApiRepository _repository;

        private IStatusLoadDataService _statusService;

        private ILoadDataRepository _loadDataRepository;

        private const int _GITHUB_LIMIT_ROWS_PAGE = 100;

        private const int _GITHUB_LIMIT_SAME_QUERY_TOTAL = 1000;

        private const int _MONTHS_TO_SEARCH_FOR_RANGE_DATE = 3;


        public LoadDataService(
            IGitHubApiRepository repository,
            IStatusLoadDataService statusService,
            ILoadDataRepository loadDataRepository)
        {
            _repository = repository;
            _loadDataRepository = loadDataRepository;
            _statusService = statusService;
        }

        public async Task LoadUsersFromLocationByMonthInvertals(string location)
        {
            DateTime dtStart = new DateTime(2008, 4, 1); //Github startup started
            DateTime dtEnd = dtStart.AddMonths(_MONTHS_TO_SEARCH_FOR_RANGE_DATE);

            do
            {
                try
                {
                    var dateRange = new DateRange(dtStart, dtEnd);

                    var result = await _repository.GetUsersFromLocationByDateRange(location, dateRange, 1, _GITHUB_LIMIT_ROWS_PAGE);

                    var dataToSave = new List<User>(result.Items);

                    _statusService.AddLoaded(location, result.TotalCount);

                    if (HaveMoreThanOnePage(result.TotalCount))
                    {
                        for (int i = 2; i <= GetNumPages(result.TotalCount); i++)
                        {
                            var resultWithPages = await _repository.GetUsersFromLocationByDateRange(location, dateRange, i, _GITHUB_LIMIT_ROWS_PAGE);

                            dataToSave.AddRange(resultWithPages.Items);

                            _statusService.AddLoaded(location, resultWithPages.TotalCount);
                        }
                    }

                    _loadDataRepository.SetData(RankingUser.BuildListFrom(dataToSave), location);
                }
                catch (Exception err)
                {
                    Debug.WriteLine(err.Message);
                }
                finally
                {
                    dtStart = dtStart.AddMonths(_MONTHS_TO_SEARCH_FOR_RANGE_DATE);
                    dtEnd = dtEnd.AddMonths(_MONTHS_TO_SEARCH_FOR_RANGE_DATE);
                }
            }
            while (dtStart <= DateTime.Now);

            _statusService.SetCalculatingOrder(location);

            await SetCommits(location);
        }

        public async Task LoadUsersFromLocation(string location)
        {
            var result = await _repository.GetUsersFromLocation(location, 1, _GITHUB_LIMIT_ROWS_PAGE);

            _statusService.SetRunning(location, result.TotalCount);

            if (CanUseTheSameQueryForAllResults(result.TotalCount))
            {
                _statusService.AddLoaded(location, result.TotalCount);

                var dataToSave = new List<User>(result.Items);

                if (HaveMoreThanOnePage(result.TotalCount))
                {
                    for (int i = 2; i <= GetNumPages(result.TotalCount); i++)
                    {
                        var resultWithPages = await _repository.GetUsersFromLocation(location, i, _GITHUB_LIMIT_ROWS_PAGE);
                        dataToSave.AddRange(resultWithPages.Items);
                    }
                }

                _loadDataRepository.SetData(RankingUser.BuildListFrom(dataToSave), location);

                _statusService.SetCalculatingOrder(location);

                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    await SetCommits(location);

                    _statusService.SetFinished(location, result.TotalCount);

                }).Start();

                return;
            }

            new Thread(async () =>
            {

                Thread.CurrentThread.IsBackground = true;

                await LoadUsersFromLocationByMonthInvertals(location);

                _statusService.SetFinished(location, result.TotalCount);

            }).Start();
        }

        private async Task SetCommits(string location)
        {
            foreach (RankingUser user in _loadDataRepository.GetDataLoaded(location))
                user.SetCommits(await _repository.GetTotalCommitsByUser(user.UserName));
        }

        private int GetNumPages(double total)
        {
            return (int)Math.Ceiling((double)total / _GITHUB_LIMIT_ROWS_PAGE);
        }

        private bool CanUseTheSameQueryForAllResults(int total)
        {
            return (total <= _GITHUB_LIMIT_SAME_QUERY_TOTAL);
        }

        private bool HaveMoreThanOnePage(int total)
        {
            return (total > _GITHUB_LIMIT_ROWS_PAGE);
        }
    }
}
