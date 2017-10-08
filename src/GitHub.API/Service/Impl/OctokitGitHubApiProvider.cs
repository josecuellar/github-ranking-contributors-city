using Newtonsoft.Json.Linq;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GitHub.API.Service.Impl
{
    public class OctokitGitHubApiProvider : IGitHubApiProvider
    {

        private const string _ACCESS_TOKEN = "8c02f531b445aa629db2efd8e03ad35e4f638941";

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
        public async Task<SearchUsersResult> GetUsersFrom(string location, DateRange dateRange, int page, int rows)
        {
            try
            {
                if (string.IsNullOrEmpty(location))
                    throw (new ArgumentNullException("userName is mandatory"));

                if (page <= 0)
                    throw (new ArgumentNullException("page not valid"));

                if (rows <= 0)
                    throw (new ArgumentNullException("rows not valid"));

                if (dateRange == null)
                    throw (new ArgumentNullException("dateRange not valid"));

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

                Debug.WriteLine(string.Format("query: location {0}, page {1}, rows{2}, date range {3} ", location, page, rows, dateRange));
                Debug.WriteLine("returned " + users.Items.Count);

                return users;
            }
            catch (RateLimitExceededException limit)
            {
                WaitForLimitRequestsPerMinute();
                return await GetUsersFrom(location, dateRange, page, rows);
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                return new SearchUsersResult();
            }

        }

        public async Task<SearchUsersResult> GetUsersFrom(string location, int page, int rows)
        {
            try
            {
                if (string.IsNullOrEmpty(location))
                    throw (new ArgumentNullException("userName is mandatory"));

                if (page <= 0)
                    throw (new ArgumentNullException("page not valid"));

                if (rows <= 0)
                    throw (new ArgumentNullException("rows not valid"));

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

                Debug.WriteLine(string.Format("query: location {0}, page {1}, rows{2}", location, page, rows));
                Debug.WriteLine("returned " + users.Items.Count);

                return users;
            }
            catch (RateLimitExceededException limit)
            {
                WaitForLimitRequestsPerMinute();
                return await GetUsersFrom(location, page, rows);
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                return new SearchUsersResult();
            }
        }

        public async Task<int> GetTotalCommitsByUser(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw (new ArgumentNullException("userName is mandatory"));

                var result = await GetClient.Connection.Get<dynamic>(
                    new Uri("https://api.github.com/search/commits?q=merge:false+author:" + userName), 
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
            catch(RateLimitExceededException limit)
            {
                WaitForLimitRequestsPerMinute();
                return await GetTotalCommitsByUser(userName);
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                return 0;
            }
        }

        public async Task<int> GetTotalRepositoriesByUser(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                    throw (new ArgumentNullException("userName is mandatory"));

                var result = await GetClient.User.Get(userName);

                var apiInfo = GetClient.Connection.GetLastApiInfo();
                var rateLimit = apiInfo?.RateLimit;

                if (rateLimit.Remaining == 0)
                    WaitForLimitRequestsPerMinute();

                return (result.PublicRepos + result.OwnedPrivateRepos + result.TotalPrivateRepos);
            }
            catch (RateLimitExceededException limit)
            {
                WaitForLimitRequestsPerMinute();
                return await GetTotalCommitsByUser(userName);
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                return 0;
            }
        }

        private void WaitForLimitRequestsPerMinute()
        {
            Debug.WriteLine("----- waitting a minute :( -----");
            Thread.Sleep(_MILLISECONDS_WAIT_FOR_AVOID_LIMIT);
        }
    }
}
