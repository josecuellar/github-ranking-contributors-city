using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.API.Repository
{
    public interface IGitHubApiRepository
    {
        Task<KeyValuePair<RateLimit, SearchUsersResult>> GetUsersFromLocationByDateRange(string location, DateRange dateRange, int page, int rows);

        Task<KeyValuePair<RateLimit, SearchUsersResult>> GetUsersFromLocation(string location, int page, int rows);

        Task<long> GetTotalCommitsByUser(string login);
    }
}
