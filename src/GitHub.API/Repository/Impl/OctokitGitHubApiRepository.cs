using Newtonsoft.Json.Linq;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GitHub.API.Repository.Impl
{
    public class OctokitGitHubApiRepository : IGitHubApiRepository
    {

        private const string _ACCESS_TOKEN = "67a32f27b93bc0cdd3150fc212fc956e82e8ad24";

        private int _MILLISECONDS_WAIT_FOR_AVOID_LIMIT = 60000; //Limit 1 minute per 30 requestsresult

        private static GitHubClient _client = null;
        private static GitHubClient GetClient
        {
            get
            {
                if (_client == null)
                {
                    _client = new GitHubClient(new Octokit.ProductHeaderValue("TestingAPI"));
                    _client.Credentials = new Credentials(_ACCESS_TOKEN);
                }                   
                return _client;
            }
        }

        //The Search API has a custom rate limit.For requests using Basic Authentication, OAuth, or client ID and secret, you can make up to 30 requests per minute.
        //Get by date range for avoid limit of 1000 results for with the same filters
        public async Task<SearchUsersResult> GetUsersFromLocationByDateRange(string location, DateRange dateRange, int page, int rows)
        {
            try
            {
                SearchUsersResult users = await GetClient.Search.SearchUsers(new SearchUsersRequest("location:" + location)
                {
                    Order = SortDirection.Descending,
                    SortField = UsersSearchSort.Repositories,
                    AccountType = AccountSearchType.User,
                    PerPage = rows,
                    Created = dateRange,
                    Page = page
                });

                var apiInfo = GetClient.GetLastApiInfo();
                var rateLimit = apiInfo?.RateLimit;

                if (rateLimit.Remaining == 0)
                    WaitForLimitRequestsPerMinute();

                return users;
            }
            catch (Exception err)
            {
                Debug.Print(err.Message);
                return new SearchUsersResult();
            }

        }

        public async Task<SearchUsersResult> GetUsersFromLocation(string location, int page, int rows)
        {
            try
            {
                SearchUsersResult users = await GetClient.Search.SearchUsers(new SearchUsersRequest("location:" + location)
                {
                    Order = SortDirection.Descending,
                    SortField = UsersSearchSort.Repositories,
                    AccountType = AccountSearchType.User,
                    PerPage = rows,
                    Page = page
                });

                var apiInfo = GetClient.GetLastApiInfo();
                var rateLimit = apiInfo?.RateLimit;

                if (rateLimit.Remaining == 0)
                    WaitForLimitRequestsPerMinute();

                return users;
            }
            catch (Exception err)
            {
                Debug.Print(err.Message);
                return new SearchUsersResult();
            }
        }

        public async Task<int> GetTotalCommitsByUser(string login)
        {
            try
            {
                var result = await GetClient.Connection.Get<dynamic>(
                    new Uri("https://api.github.com/search/commits?q=author-name:" + login), 
                    new Dictionary<string, string>(), 
                    "application/vnd.github.cloak-preview");

                var apiInfo = GetClient.Connection.GetLastApiInfo();
                var rateLimit = apiInfo?.RateLimit;

                if (rateLimit.Remaining == 0)
                    WaitForLimitRequestsPerMinute();

                JObject jobject = JObject.Parse(result.Body.ToString());
                JToken token = jobject.First;
                JToken nodeValue = ((JProperty)token).Value;

                if (int.TryParse(nodeValue.ToString(), out var toreturn))
                    return toreturn;

                return 0;
            }
            catch (Exception err)
            {
                Debug.Print(err.Message);
                return 0;
            }
        }

        private void WaitForLimitRequestsPerMinute()
        {
            Debug.Print("----- waitting a minute :( -----");
            Thread.Sleep(_MILLISECONDS_WAIT_FOR_AVOID_LIMIT);
        }
    }
}
