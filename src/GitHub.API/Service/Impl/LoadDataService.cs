using Microsoft.Extensions.Caching.Memory;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GitHub.API.Repository.Impl
{
    public class LoadDataService : ILoadDataService
    {

        private IGitHubApiRepository _repository;

        private IMemoryCache _memoryCache;

        private string _KEYCACHE_USERS = "GitHubUsers";

        private int _MILLISECONDS_WAIT_FOR_AVOID_LIMIT = 60000; //Limit 1 minute per 30 requestsresult

        private const int _GITHUB_LIMIT_ROWS_PAGE = 100;

        private const int _MONTHS_TO_SEARCH_FOR_RANGE_DATE = 4;


        public LoadDataService(IGitHubApiRepository repository, IMemoryCache memoryCache)
        {
            _repository = repository;
            _memoryCache = memoryCache;
        }


        public async Task LoadUsersFromLocationAndPersist(string location)
        {
            DateTime dtStart = new DateTime(2008, 4, 1); //Github startup started

            DateTime dtEnd = dtStart.AddMonths(_MONTHS_TO_SEARCH_FOR_RANGE_DATE);

            _memoryCache.Set(_KEYCACHE_USERS, new List<User>());

            while (dtStart <= DateTime.Now)
            {
                try
                {
                    var result = await _repository.GetUsersWithMoreRepositoriesFromLocation(location, new DateRange(dtStart, dtEnd), 1, _GITHUB_LIMIT_ROWS_PAGE);

                    if (result.Key.Remaining == 0)
                        Thread.Sleep(_MILLISECONDS_WAIT_FOR_AVOID_LIMIT); 

                    _memoryCache.Get<List<Octokit.User>>(_KEYCACHE_USERS).AddRange(new List<Octokit.User>(result.Value.Items));

                    if (result.Value.TotalCount > _GITHUB_LIMIT_ROWS_PAGE)
                    {

                        int numPages = (int)Math.Ceiling((double)result.Value.TotalCount / _GITHUB_LIMIT_ROWS_PAGE);

                        for (int i = 2; i <= numPages; i++)
                        {
                            var resultWithPages = await _repository.GetUsersWithMoreRepositoriesFromLocation(location, new DateRange(dtStart, dtEnd), i, _GITHUB_LIMIT_ROWS_PAGE);

                            if (resultWithPages.Key.Remaining == 0)
                                Thread.Sleep(_MILLISECONDS_WAIT_FOR_AVOID_LIMIT);

                            _memoryCache.Get<List<Octokit.User>>(_KEYCACHE_USERS).AddRange(new List<Octokit.User>(resultWithPages.Value.Items));
                        }

                    }
                }
                catch (Exception err)
                {
                    Debug.WriteLine(err.Message);
                }
                finally
                {
                    dtStart = dtStart.AddMonths(_MONTHS_TO_SEARCH_FOR_RANGE_DATE);
                    dtEnd = dtEnd.AddMonths(_MONTHS_TO_SEARCH_FOR_RANGE_DATE);
                    Debug.WriteLine(_memoryCache.Get<List<Octokit.User>>(_KEYCACHE_USERS).Count);
                }

            }
        }        
    }
}
