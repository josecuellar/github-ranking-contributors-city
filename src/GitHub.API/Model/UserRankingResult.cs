using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHub.API.Model
{
    public class UserRankingResult
    {
        public List<Octokit.User> Users { get; private set; }

        public bool IncompleteResults { get; private set; }

        public DateTime ResultsLoadedUntil { get; private set; }

        public long TotalLoadedResult { get; private set; }

        public UserRankingResult(
            List<Octokit.User> users, 
            bool incompleteResults, 
            DateTime resultsLoadedUntil,
            long totalLoadedResult)
        {
            Users = users;
            IncompleteResults = incompleteResults;
            ResultsLoadedUntil = resultsLoadedUntil;
            TotalLoadedResult = totalLoadedResult;
        }
    }
}
