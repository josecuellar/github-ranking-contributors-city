using GitHub.API.Model;
using GitHub.API.Repository;
using Microsoft.Extensions.Caching.Memory;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static GitHub.API.Model.LoadStatus;

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

                    _loadDataRepository.SetData(RankingUser.BuildListFrom(result.Value.Items), location);
                    _statusService.SetStatus(location, StatusItems.RUNNING, _statusService.GetStatus(location).TotalResultsLoaded + result.Value.TotalCount);

                    if (result.Value.TotalCount > _GITHUB_LIMIT_ROWS_PAGE)
                    {
                        int numPages = (int)Math.Ceiling((double)result.Value.TotalCount / _GITHUB_LIMIT_ROWS_PAGE);

                        for (int i = 2; i <= numPages; i++)
                        {
                            var resultWithPages = await _repository.GetUsersFromLocationByDateRange(location, dateRange, i, _GITHUB_LIMIT_ROWS_PAGE);
                            _loadDataRepository.SetData(RankingUser.BuildListFrom(resultWithPages.Value.Items), location);
                        }
                    }

                    SetTotalCommits(location);

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

        }

        public async Task LoadUsersFromLocation(string location)
        {
            var result = await _repository.GetUsersFromLocation(location, 1, _GITHUB_LIMIT_ROWS_PAGE);

            if (result.Value.TotalCount <= _GITHUB_LIMIT_SAME_QUERY_TOTAL)
            {

                _loadDataRepository.SetData(RankingUser.BuildListFrom(result.Value.Items), location);

                int numPages = (int)Math.Ceiling((double)result.Value.TotalCount / _GITHUB_LIMIT_ROWS_PAGE);

                for (int i = 2; i <= numPages; i++)
                {
                    var resultWithPages = await _repository.GetUsersFromLocation(location, i, _GITHUB_LIMIT_ROWS_PAGE);
                    _loadDataRepository.SetData(RankingUser.BuildListFrom(resultWithPages.Value.Items), location);
                    _statusService.SetStatus(location, StatusItems.RUNNING, result.Value.TotalCount);
                }

                SetTotalCommits(location);

                _statusService.SetStatus(location, StatusItems.FINISHED, result.Value.TotalCount, result.Value.TotalCount);

                return;
            }

            await LoadUsersFromLocationByMonthInvertals(location);
        }

        private void SetTotalCommits(string location)
        {
            _loadDataRepository.GetDataLoadedMutable(location).ForEach(async x => x.TotalCommits = await _repository.GetTotalCommitsByUser(x.UserName));
        }
    }
}
