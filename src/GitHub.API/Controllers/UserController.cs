using GitHub.API.Model;
using GitHub.API.Repository;
using GitHub.API.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GitHub.API.Controllers
{
    public class UsersController : Controller
    {

        private ILoadDataService _service;
        private IStatusLoadDataService _serviceStatus;
        private ILoadDataRepository _repository;

        public UsersController(ILoadDataService service, 
            IStatusLoadDataService serviceStatus,
            ILoadDataRepository repository)
        {
            _service = service;
            _serviceStatus = serviceStatus;
            _repository = repository;
        }

        [HttpGet]
        [Route("api/user/{location}/{top}")]
        public async Task<IActionResult> Get(string location, int top)
        {
            if (string.IsNullOrEmpty(location))
                return BadRequest("location is mandatory");

            if (top > 150)
                return BadRequest("max value for top is 150");

            if (_serviceStatus.GetStatus(location).Status == Model.LoadStatus.StatusItems.STOPPED)
            {
                //new Thread(async () =>
                //{
                    //Thread.CurrentThread.IsBackground = true;
                    await _service.LoadUsersFromLocation(location);
                //}).Start();
            }

            var loadStatus = _serviceStatus.GetStatus(location);

            var dataLoaded = _repository.GetDataLoaded(location);

            return Ok(new UserRankingResult(dataLoaded, loadStatus, top));
            
        }
    }
}
