using Newtonsoft.Json.Linq;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GitHub.API.Service.Impl
{
    public class OctokitGitHubApiProvider : IGitHubApiProvider
    {

        private const string ACCESS_TOKEN = "XXXXXXXXXXXXXXXX";

        private int MILLISECONDS_WAIT_FOR_AVOID_LIMIT = 60000; 

        private static GitHubClient _client = null;
        private static GitHubClient GetClient
        {
            get
            {
                if (_client == null)
                {
                    _client = new GitHubClient(new Octokit.ProductHeaderValue("TestingAPI"));
                    _client.Credentials = new Credentials(ACCESS_TOKEN);
                }                   
                return _client;
            }
        }

        public async Task<SearchUsersResult> GetUsersFrom(string location, DateRange dateRange, int page, int rows)
        {
            try
            {
                if (string.IsNullOrEmpty(location))
                    throw (new ArgumentNullException("location is mandatory"));

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
                    Repositories = new Range(1, int.MaxValue),
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
                    throw (new ArgumentNullException("location is mandatory"));

                if (page <= 0)
                    throw (new ArgumentNullException("page not valid"));

                if (rows <= 0)
                    throw (new ArgumentNullException("rows not valid"));

                SearchUsersResult users = await GetClient.Search.SearchUsers(new SearchUsersRequest("location:" + location)
                {
                    Order = SortDirection.Descending,
                    SortField = UsersSearchSort.Repositories,
                    AccountType = AccountSearchType.User,
                    Repositories = new Range(1, int.MaxValue),
                    PerPage = rows,
                    Page = page
                });

                var apiInfo = GetClient.GetLastApiInfo();
                var rateLimit = apiInfo?.RateLimit;

                if (rateLimit.Remaining == 0)
                    WaitForLimitRequestsPerMinute();

                return users;
            }
            catch (RateLimitExceededException limit)
            {
                Debug.WriteLine(limit.Message);
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
                Debug.WriteLine(limit.Message);
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
                Debug.WriteLine(limit.Message);
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
            Debug.WriteLine("---- WAITING :( ----");
            Thread.Sleep(MILLISECONDS_WAIT_FOR_AVOID_LIMIT);
        }
    }
}
