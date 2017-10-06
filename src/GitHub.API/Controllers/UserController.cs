using GitHub.API.Model;
using GitHub.API.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace GitHub.API.Controllers
{
    public class UsersController : Controller
    {

        private ILoadDataService _service;

        public UsersController(ILoadDataService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("api/users/{location}/{top?}")]
        public UserRankingResult Get(string location)
        {
            if (_service.GetStatus(location).Status == Model.LoadStatus.StatusItems.STOPPED)
            {
                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await _service.LoadUsersFromLocation(location);
                }).Start();
            }

            var loadStatus = _service.GetStatus(location);

            var dataLoaded = _service.GetDataLoaded(location);

            return new UserRankingResult(
                (dataLoaded.Count > 100 ? dataLoaded.GetRange(0, 100) : dataLoaded), 
                (loadStatus.Status == LoadStatus.StatusItems.RUNNING), 
                loadStatus.LoadedUntil, dataLoaded.Count);
        }
    }
}
