using Newtonsoft.Json.Linq;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitHub.API.Repository.Impl
{
    public class OctokitGitHubApiRepository : IGitHubApiRepository
    {

        private const string _ACCESS_TOKEN = "cd9734b2e3d3394004da0d70bcdcb90c1bc78024";

        private int _MILLISECONDS_WAIT_FOR_AVOID_LIMIT = 60000; //Limit 1 minute per 30 requestsresult

        //The Search API has a custom rate limit.For requests using Basic Authentication, OAuth, or client ID and secret, you can make up to 30 requests per minute.
        //Get by date range for avoid limit of 1000 results for with the same filters
        public async Task<KeyValuePair<RateLimit, SearchUsersResult>> GetUsersFromLocationByDateRange(string location, DateRange dateRange, int page, int rows)
        {
            try
            {
                var githubClient = GetClient();

                SearchUsersResult users = await githubClient.Search.SearchUsers(new SearchUsersRequest("location:" + location)
                {
                    Order = SortDirection.Descending,
                    SortField = UsersSearchSort.Repositories,
                    AccountType = AccountSearchType.User,
                    PerPage = rows,
                    Created = dateRange,
                    Page = page
                });

                var apiInfo = githubClient.GetLastApiInfo();
                var rateLimit = apiInfo?.RateLimit;

                if (rateLimit.Remaining == 0)
                    Thread.Sleep(_MILLISECONDS_WAIT_FOR_AVOID_LIMIT);

                return new KeyValuePair<RateLimit, SearchUsersResult>(rateLimit, users);
            }
            catch (Exception err)
            {
                Debug.Print(err.Message);
                return new KeyValuePair<RateLimit, SearchUsersResult>(new RateLimit(), new SearchUsersResult());
            }

        }

        public async Task<KeyValuePair<RateLimit, SearchUsersResult>> GetUsersFromLocation(string location, int page, int rows)
        {
            try
            {
                var githubClient = GetClient();

                SearchUsersResult users = await githubClient.Search.SearchUsers(new SearchUsersRequest("location:" + location)
                {
                    Order = SortDirection.Descending,
                    SortField = UsersSearchSort.Repositories,
                    AccountType = AccountSearchType.User,
                    PerPage = rows,
                    Page = page
                });

                var apiInfo = githubClient.GetLastApiInfo();
                var rateLimit = apiInfo?.RateLimit;

                if (rateLimit.Remaining == 0)
                    Thread.Sleep(_MILLISECONDS_WAIT_FOR_AVOID_LIMIT);

                return new KeyValuePair<RateLimit, SearchUsersResult>(rateLimit, users);
            }
            catch (Exception err)
            {
                Debug.Print(err.Message);
                return new KeyValuePair<RateLimit, SearchUsersResult>(new RateLimit(), new SearchUsersResult());
            }

        }


        public async Task<long> GetTotalCommitsByUser(string login)
        {
            try
            {
                var githubClient = GetClient();

                var result = await githubClient.Connection.Get<dynamic>(
                    new Uri("https://api.github.com/search/commits?q=commiter:" + login), 
                    new Dictionary<string, string>() { { "sort", "committer-date" }, { "order", "asc" } }, 
                    "application/vnd.github.cloak-preview");

                var apiInfo = githubClient.GetLastApiInfo();
                var rateLimit = apiInfo?.RateLimit;

                if (rateLimit.Remaining == 0)
                    Thread.Sleep(_MILLISECONDS_WAIT_FOR_AVOID_LIMIT);

                JObject jobject = JObject.Parse(result.Body.ToString());
                JToken token = jobject.First;
                JToken nodeValue = ((JProperty)token).Value;

                if (long.TryParse(nodeValue.ToString(), out var toreturn))
                    return toreturn;

                return 0;
            }
            catch (Exception err)
            {
                Debug.Print(err.Message);
                return 0;
            }
        }


        private GitHubClient GetClient()
        {
            var githubClient = new GitHubClient(new Octokit.ProductHeaderValue("TestingAPI"));
            githubClient.Credentials = new Credentials(_ACCESS_TOKEN);
            return githubClient;
        }
    }
}
