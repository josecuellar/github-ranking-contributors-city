using GitHub.API.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GitHub.API.Model.LoadStatus;

namespace GitHub.API.Repository
{
    public interface ILoadDataService
    {
        Task LoadUsersFromLocation(string location);

        Task LoadUsersFromLocationByMonthInvertals(string location);

        LoadStatus GetStatus(string location);

        void SetStatus(string location, StatusItems status, long totalResultsLoaded, long totalResults);

        void SetStatus(string location, StatusItems status, long totalResultsLoaded);

        List<RankingUser> GetDataLoaded(string location);
    }
}
