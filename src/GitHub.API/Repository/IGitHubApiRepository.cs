using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.API.Repository
{
    public interface IGitHubApiRepository
    {
        Task<SearchUsersResult> GetUsersFromLocationByDateRange(string location, DateRange dateRange, int page, int rows);

        Task<SearchUsersResult> GetUsersFromLocation(string location, int page, int rows);

        Task<int> GetTotalCommitsByUser(string login);
    }
}
