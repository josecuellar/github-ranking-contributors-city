using System.Threading.Tasks;

namespace GitHub.API.Repository
{
    public interface ILoadDataService
    {
        Task LoadUsersFromLocationAndPersist(string location);
    }
}
