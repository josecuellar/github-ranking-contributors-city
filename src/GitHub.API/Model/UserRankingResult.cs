using System.Collections.Generic;
using System.Linq;

namespace GitHub.API.Model
{
    public class UserRankingResult
    {
        public LoadStatus Status { get; private set; }

        public List<RankingUser> Users { get; private set; }

        public UserRankingResult(List<RankingUser> users, LoadStatus status)
        {
            Users = users;
            Status = status;
        }
    }
}
