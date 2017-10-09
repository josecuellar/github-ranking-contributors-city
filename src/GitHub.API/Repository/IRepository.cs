using GitHub.API.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitHub.API.Repository
{
    public interface IRepository
    {
        ConcurrentDictionary<string, RankingUser> Get(string location);

        List<RankingUser> GetOrderedByReposAndCommits(string location, int topResults);

        int Set(ConcurrentDictionary<string, RankingUser> users, string location);

        void SetReposAndCommitsToUser(string location, string userName, int commits, int repositories);
    }
}
