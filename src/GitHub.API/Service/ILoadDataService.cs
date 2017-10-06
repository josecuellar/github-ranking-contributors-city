using GitHub.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.API.Repository
{
    public interface ILoadDataService
    {
        Task LoadUsersFromLocation(string location);

        LoadStatus GetStatus(string location);

        List<Octokit.User> GetDataLoaded(string location);
    }
}
