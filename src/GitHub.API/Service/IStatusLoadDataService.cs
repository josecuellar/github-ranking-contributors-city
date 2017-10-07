using GitHub.API.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GitHub.API.Model.LoadStatus;

namespace GitHub.API.Service
{
    public interface IStatusLoadDataService
    {
        LoadStatus GetStatus(string location);

        void SetStatus(string location, StatusItems status, long totalResultsLoaded, long totalResults);

        void SetStatus(string location, StatusItems status, long totalResultsLoaded);
    }
}
