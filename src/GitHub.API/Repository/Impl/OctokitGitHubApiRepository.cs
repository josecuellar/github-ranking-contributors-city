using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GitHub.API.Repository.Impl
{
    public class OctokitGitHubApiRepository : IGitHubApiRepository
    {


        private const string _GITHUB_BASE_URL = "https://api.github.com/search/users";

        //The Search API has a custom rate limit.For requests using Basic Authentication, OAuth, or client ID and secret, you can make up to 30 requests per minute.
        //Get by date range for avoid limit of 1000 results for with the same filters
        public async Task<KeyValuePair<RateLimit, SearchUsersResult>> GetUsersWithMoreRepositoriesFromLocation(string location, DateRange dateRange, int page, int rows)
        {
            try
            {
                var githubClient = new GitHubClient(new Octokit.ProductHeaderValue("TestingAPI"));
                githubClient.Credentials = new Credentials("b80797ac81bb6774f58f6562d710ebeca70e0fcb");


                SearchUsersResult users = await githubClient.Search.SearchUsers(new SearchUsersRequest("location:" + location) {
                    Order = SortDirection.Descending,
                    SortField = UsersSearchSort.Repositories,
                    AccountType = AccountSearchType.User,
                    PerPage = rows,
                    Created = dateRange,
                    Page = page
                });

                var apiInfo = githubClient.GetLastApiInfo();
                var rateLimit = apiInfo?.RateLimit;

                return new KeyValuePair<RateLimit, SearchUsersResult>(rateLimit, users);
            }
            catch (Exception err)
            {
                Debug.Print(err.Message);
                return new KeyValuePair<RateLimit, SearchUsersResult>(new RateLimit(), new SearchUsersResult());
            }

        }        
    }
}
