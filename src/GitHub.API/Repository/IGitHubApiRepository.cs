using GitHub.API.Model;
using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.API.Repository
{
    public interface IGitHubApiRepository
    {
        Task<KeyValuePair<RateLimit, SearchUsersResult>> GetUsersWithMoreRepositoriesFromLocation(string location, DateRange dateRange, int page, int rows);
    }
}
