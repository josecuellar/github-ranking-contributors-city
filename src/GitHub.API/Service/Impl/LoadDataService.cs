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

        private IPersistenceLocalDataRepository _persistenceRepository;

        private IMemoryCache _memoryCache;

        private int _MILLISECONDS_WAIT_FOR_AVOID_LIMIT = 60000; //Limit 1 minute per 30 requestsresult

        private const int _GITHUB_LIMIT_ROWS_PAGE = 100;

        private const int _MONTHS_TO_SEARCH_FOR_RANGE_DATE = 3;


        public LoadDataService(
            IGitHubApiRepository repository, 
            IMemoryCache memoryCache, 
            IPersistenceLocalDataRepository persistenceRepository)
        {
            _repository = repository;
            _memoryCache = memoryCache;
            _persistenceRepository = persistenceRepository;
        }


        public async Task LoadUsersFromLocationAndPersist(string location)
        {
            DateTime dtStart = new DateTime(2008, 4, 1); //Github startup started

            DateTime dtEnd = dtStart.AddMonths(_MONTHS_TO_SEARCH_FOR_RANGE_DATE);

            do
            {
                try
                {
                    var result = await _repository.GetUsersWithMoreRepositoriesFromLocation(location, new DateRange(dtStart, dtEnd), 1, _GITHUB_LIMIT_ROWS_PAGE);

                    CheckLimitGithubRemaining(result.Key);
                    Persist(new List<Octokit.User>(result.Value.Items), location);

                    if (result.Value.TotalCount > _GITHUB_LIMIT_ROWS_PAGE)
                    {

                        int numPages = (int)Math.Ceiling((double)result.Value.TotalCount / _GITHUB_LIMIT_ROWS_PAGE);

                        for (int i = 2; i <= numPages; i++)
                        {
                            var resultWithPages = await _repository.GetUsersWithMoreRepositoriesFromLocation(location, new DateRange(dtStart, dtEnd), i, _GITHUB_LIMIT_ROWS_PAGE);

                            CheckLimitGithubRemaining(result.Key);
                            Persist(new List<Octokit.User>(resultWithPages.Value.Items), location);
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
                }
            }
            while (dtStart <= DateTime.Now);
        }

        private void Persist(List<Octokit.User> users, string location)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                _persistenceRepository.CreateListIfNotExists(users.ConvertAll<Model.User>(item => new Model.User(item.Id, item.Login, location, item.HtmlUrl, 0)));
            }).Start();
        }

        private void CheckLimitGithubRemaining(RateLimit limits)
        {
            if (limits.Remaining == 0)
                Thread.Sleep(_MILLISECONDS_WAIT_FOR_AVOID_LIMIT);
        }
    }
}
