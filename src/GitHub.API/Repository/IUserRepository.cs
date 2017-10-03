using GitHub.API.Model;
using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.API.Repository
{
    public interface IUserRepository
    {
        Task<SearchUsersResult> GetUsersWithMoreRepositoriesFromLocation(string location, int page);
    }
}
