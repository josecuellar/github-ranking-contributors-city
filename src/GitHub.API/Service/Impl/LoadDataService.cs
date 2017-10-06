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

        private const int _MONTHS_TO_SEARCH_FOR_RANGE_DATE = 3;


        public LoadDataService(
            IGitHubApiRepository repository, 
            IMemoryCache memoryCache)
        {
            _repository = repository;
            _memoryCache = memoryCache;
        }

        public async Task LoadUsersFromLocation(string location)
        {
            DateTime dtStart = new DateTime(2008, 4, 1); //Github startup started

            DateTime dtEnd = dtStart.AddMonths(_MONTHS_TO_SEARCH_FOR_RANGE_DATE);

            do
            {
                try
                {
                    var result = await _repository.GetUsersWithMoreRepositoriesFromLocation(location, new DateRange(dtStart, dtEnd), 1, _GITHUB_LIMIT_ROWS_PAGE);

                    CheckLimitGithubRemaining(result.Key);
                    SetMemory(result.Value.Items, location);

                    if (result.Value.TotalCount > _GITHUB_LIMIT_ROWS_PAGE)
                    {
                        int numPages = (int)Math.Ceiling((double)result.Value.TotalCount / _GITHUB_LIMIT_ROWS_PAGE);

                        for (int i = 2; i <= numPages; i++)
                        {
                            var resultWithPages = await _repository.GetUsersWithMoreRepositoriesFromLocation(location, new DateRange(dtStart, dtEnd), i, _GITHUB_LIMIT_ROWS_PAGE);

                            CheckLimitGithubRemaining(result.Key);
                            SetMemory(resultWithPages.Value.Items, location);
                        }
                    }

                    SetStatus(location, StatusItems.RUNNING, dtEnd);

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

            SetStatus(location, StatusItems.FINISHED, dtEnd);

        }

        public LoadStatus GetStatus(string location)
        {
            return (_memoryCache.TryGetValue<LoadStatus>("Status" + location, out var toReturn) ? toReturn : new LoadStatus());
        }
        public List<Octokit.User> GetDataLoaded(string location)
        {
            return (_memoryCache.TryGetValue<List<Octokit.User>>(location, out var toReturn) ? toReturn : new List<User>());
        }

        private void SetMemory(IReadOnlyList<User> users, string location)
        {
            if (users == null || (users != null && users.Count == 0))
                return;

            if (_memoryCache.Get(location) == null)
                _memoryCache.Set(location, new List<User>());

            if (_memoryCache.TryGetValue<List<User>>(location, out var usersMemory))
                usersMemory.AddRange(users);
        }

        private void SetStatus(string location, StatusItems status, DateTime loadedUntil)
        {
            _memoryCache.Set("Status" + location, new LoadStatus(loadedUntil, status));
        }
        
        private void CheckLimitGithubRemaining(RateLimit limits)
        {
            if (limits.Remaining == 0)
                Thread.Sleep(_MILLISECONDS_WAIT_FOR_AVOID_LIMIT);
        }
    }
}
