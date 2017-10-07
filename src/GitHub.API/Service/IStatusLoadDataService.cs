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

        void AddLoaded(string location, long loaded);

        void SetRunning(string location, long total);

        void SetRunning(string location);

        void SetFinished(string location, long total);

        void SetCalculatingOrder(string location);
    }
}
