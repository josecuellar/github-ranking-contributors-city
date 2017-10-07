using GitHub.API.Model;
using GitHub.API.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        [Route("api/user/{location}/{top}")]
        public async Task<IActionResult> Get(string location, int top)
        {
            if (string.IsNullOrEmpty(location))
                return BadRequest("location is mandatory");

            if (top > 150)
                return BadRequest("max value for top is 150");

            if (_service.GetStatus(location).Status == Model.LoadStatus.StatusItems.STOPPED)
            {
                //new Thread(async () =>
                //{
                    //Thread.CurrentThread.IsBackground = true;
                    await _service.LoadUsersFromLocation(location);
                //}).Start();
            }

            var loadStatus = _service.GetStatus(location);
            var dataLoaded = _service.GetDataLoaded(location);

            return Ok(new UserRankingResult(dataLoaded, loadStatus, top));
            
        }
    }
}
