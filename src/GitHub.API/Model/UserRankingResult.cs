using System.Collections.Generic;

namespace GitHub.API.Model
{
    public class UserRankingResult
    {
        public LoadStatus Status { get; private set; }

        public List<RankingUser> Users { get; private set; }

        public UserRankingResult(List<RankingUser> users, LoadStatus status, int topResults)
        {
            if (users.Count > topResults)
                users = users.GetRange(0, topResults);

            Users = users;
            Status = status;
        }
    }
}
