using System.Collections.Generic;

namespace GitHub.API.Model
{
    public class RankingUser 
    {
        private static long order = 0;

        public long Order { get; set; }

        public long TotalCommits { get; set; }

        public string UserName { get; set; }

        public string Url { get; set; }

        public string Location { get; set; }

        public RankingUser(long totalCommits, string userName, string url, string location) 
        {
            TotalCommits = totalCommits;
            UserName = userName;
            Url = url;
            Order = order++;
            Location = location;
        }

        public static IReadOnlyList<RankingUser> BuildListFrom(IReadOnlyList<Octokit.User> baseUsers)
        {
            return new List<RankingUser>(new List<Octokit.User>(baseUsers).ConvertAll(x => new RankingUser(0, x.Login, x.HtmlUrl, x.Location)));
        }
    }
}
