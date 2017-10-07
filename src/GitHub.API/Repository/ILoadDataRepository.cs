using GitHub.API.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GitHub.API.Model.LoadStatus;

namespace GitHub.API.Repository
{
    public interface ILoadDataRepository
    {
        List<RankingUser> GetDataLoaded(string location);

        List<RankingUser> GetDataLoadedMutable(string location);

        void SetData(IReadOnlyList<RankingUser> users, string location);
    }
}
