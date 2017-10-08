using Octokit;
using System.Threading.Tasks;

namespace GitHub.API.Service
{
    public interface IGitHubApiProvider
    {
        Task<SearchUsersResult> GetUsersFrom(string location, DateRange dateRange, int page, int rows);

        Task<SearchUsersResult> GetUsersFrom(string location, int page, int rows);

        Task<int> GetTotalCommitsByUser(string login);

        Task<int> GetTotalRepositoriesByUser(string login);
    }
}
