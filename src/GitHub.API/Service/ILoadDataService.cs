using GitHub.API.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GitHub.API.Model.LoadStatus;

namespace GitHub.API.Service
{
    public interface ILoadDataService
    {
        Task LoadUsersFromLocation(string location);

        Task LoadUsersFromLocationByMonthInvertals(string location);
    }
}
