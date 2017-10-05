using GitHub.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.API.Repository
{
    public interface IPersistenceLocalDataRepository
    {
        void SaveList(List<Model.User> users);

        Task Save(User user);

        void CreateListIfNotExists(List<Model.User> users);

        Task CreateIfNotExists(Model.User users);
    }
}
