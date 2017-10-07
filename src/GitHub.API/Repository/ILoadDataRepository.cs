using GitHub.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.API.Repository
{
    public interface ILoadDataRepository
    {
        List<RankingUser> GetDataLoaded(string location);

        void SetData(IReadOnlyList<RankingUser> users, string location);

        List<RankingUser> GetDataLoadedOrderedByReposAndCommits(string location, int topResults);
    }
}
