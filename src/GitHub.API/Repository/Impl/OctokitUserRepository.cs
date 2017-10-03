using GitHub.API.Model;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GitHub.API.Repository.Impl
{
    public class OctokitAPIGitHubUserRepository : IUserRepository
    {
        private const string _GITHUB_BASE_URL = "https://api.github.com/search/users";
        private const int _GITHUB_LIMIT_ROWS_PAGE = 100;

        public async Task<SearchUsersResult> GetUsersWithMoreRepositoriesFromLocation(string location, int page)
        {
            try
            {
                var github = new GitHubClient(new Octokit.ProductHeaderValue("TestingAPIGithub"));
                SearchUsersResult users = await github.Search.SearchUsers(new SearchUsersRequest("location:" + location) {
                    Order = SortDirection.Descending,
                    SortField = UsersSearchSort.Repositories,
                    AccountType = AccountSearchType.User,
                    PerPage = _GITHUB_LIMIT_ROWS_PAGE,
                    Page = page
                });
                return users;
            }
            catch (Exception err)
            {
                Debug.Print(err.Message);
                return new SearchUsersResult();
            }

        }
    }
}
