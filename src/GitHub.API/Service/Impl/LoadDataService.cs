using GitHub.API.Model;
using Microsoft.Extensions.Caching.Memory;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static GitHub.API.Model.LoadStatus;

namespace GitHub.API.Repository.Impl
{
    public class LoadDataService : ILoadDataService
    {

        private IGitHubApiRepository _repository;

        private IMemoryCache _memoryCache;

        private int _MILLISECONDS_WAIT_FOR_AVOID_LIMIT = 60000; //Limit 1 minute per 30 requestsresult

        private const int _GITHUB_LIMIT_ROWS_PAGE = 100;

        private const int _GITHUB_LIMIT_SAME_QUERY_TOTAL = 1000;

        private const int _MONTHS_TO_SEARCH_FOR_RANGE_DATE = 3;


        public LoadDataService(
            IGitHubApiRepository repository, 
            IMemoryCache memoryCache)
        {
            _repository = repository;
            _memoryCache = memoryCache;
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

                    CheckLimitGithubRemaining(result.Key);
                    SetData(RankingUser.BuildListFrom(result.Value.Items), location);
                    SetStatus(location, StatusItems.RUNNING, GetStatus(location).TotalResultsLoaded + result.Value.TotalCount);

                    if (result.Value.TotalCount > _GITHUB_LIMIT_ROWS_PAGE)
                    {
                        int numPages = (int)Math.Ceiling((double)result.Value.TotalCount / _GITHUB_LIMIT_ROWS_PAGE);

                        for (int i = 2; i <= numPages; i++)
                        {
                            var resultWithPages = await _repository.GetUsersFromLocationByDateRange(location, dateRange, i, _GITHUB_LIMIT_ROWS_PAGE);

                            CheckLimitGithubRemaining(result.Key);
                            SetData(RankingUser.BuildListFrom(resultWithPages.Value.Items), location);
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

                CheckLimitGithubRemaining(result.Key);
                SetData(RankingUser.BuildListFrom(result.Value.Items), location);

                int numPages = (int)Math.Ceiling((double)result.Value.TotalCount / _GITHUB_LIMIT_ROWS_PAGE);

                for (int i = 2; i <= numPages; i++)
                {
                    var resultWithPages = await _repository.GetUsersFromLocation(location, i, _GITHUB_LIMIT_ROWS_PAGE);

                    CheckLimitGithubRemaining(result.Key);
                    SetData(RankingUser.BuildListFrom(resultWithPages.Value.Items), location);
                    SetStatus(location, StatusItems.RUNNING, result.Value.TotalCount);
                }

                SetTotalCommits(location);

                SetStatus(location, StatusItems.FINISHED, result.Value.TotalCount, result.Value.TotalCount);

                return;
            }

            await LoadUsersFromLocationByMonthInvertals(location);
        }

        public LoadStatus GetStatus(string location)
        {
            if (!_memoryCache.TryGetValue<LoadStatus>("Status" + location, out var toReturn))
                _memoryCache.Set("Status" + location, new LoadStatus());

            return _memoryCache.Get<LoadStatus>("Status" + location);
        }

        public void SetStatus(string location, StatusItems status, long totalResultsLoaded, long totalResults)
        {
            GetStatus(location).Status = status;
            GetStatus(location).TotalResults = totalResults;
            GetStatus(location).TotalResultsLoaded = totalResultsLoaded;
        }

        public void SetStatus(string location, StatusItems status, long totalResultsLoaded)
        {
            GetStatus(location).Status = status;
            GetStatus(location).TotalResultsLoaded = totalResultsLoaded;
        }

        public List<RankingUser> GetDataLoaded(string location)
        {
            List<RankingUser> dataLoaded = (_memoryCache.TryGetValue<List<RankingUser>>(location, out var toReturn) ? toReturn : new List<RankingUser>());
            return new List<RankingUser>(dataLoaded);
        }

        private void SetData(IReadOnlyList<RankingUser> users, string location)
        {
            if (users == null || (users != null && users.Count == 0))
                return;

            if (_memoryCache.Get(location) == null)
                _memoryCache.Set(location, new List<RankingUser>());

            if (_memoryCache.TryGetValue<List<RankingUser>>(location, out var usersMemory))
                usersMemory.AddRange(users);
        }

        private List<RankingUser> GetDataLoadedMutable(string location)
        {
            return (_memoryCache.TryGetValue<List<RankingUser>>(location, out var toReturn) ? toReturn : new List<RankingUser>());
        }

        private void SetTotalCommits(string location)
        {
            GetDataLoadedMutable(location).ForEach(async x => x.TotalCommits = await _repository.GetTotalCommitsByUser(x.UserName));
        }

        private void CheckLimitGithubRemaining(RateLimit limits)
        {
            if (limits.Remaining == 0)
                Thread.Sleep(_MILLISECONDS_WAIT_FOR_AVOID_LIMIT);
        }
    }
}
