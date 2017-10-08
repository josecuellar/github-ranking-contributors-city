using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GitHub.API.Model
{
    public class RankingUser 
    {
        public string UserName { get; private set; }

        public string Url { get; private set; }

        public int Repositories { get; set; }

        public int Commits { get; set; }

        public RankingUser(string userName, string url, int repositories) 
        {
            UserName = userName;
            Url = url;
            Repositories = repositories;
            Commits = 0;
        }

        public static List<RankingUser> BuildListFrom(IReadOnlyList<Octokit.User> users)
        {
            return new List<RankingUser>(new List<Octokit.User>(users).ConvertAll(x => new RankingUser(x.Login, x.HtmlUrl, x.TotalPrivateRepos)));
        }

        public static ConcurrentDictionary<string, RankingUser> BuildConcurrentDictionaryFrom(IReadOnlyList<Octokit.User> users)
        {
            var listUsers = new List<Octokit.User>(users).ConvertAll(x => new RankingUser(x.Login, x.HtmlUrl, x.TotalPrivateRepos));
            return new ConcurrentDictionary<string, RankingUser>(listUsers.ToDictionary(key=>key.UserName, val=>val));
        }

    }
}
