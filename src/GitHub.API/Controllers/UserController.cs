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
        private IStatusRepository _serviceStatus;
        private IRepository _repository;

        public UsersController(ILoadDataService service, 
            IStatusRepository serviceStatus,
            IRepository repository)
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

            if (_serviceStatus.Get(location).Status == Model.LoadStatus.StatusItems.STOPPED)
                await _service.LoadUsersFromLocation(location);
            
            var loadStatus = _serviceStatus.Get(location);

            var dataLoaded = _repository.GetOrderedByReposAndCommits(location, top);

            return Ok(new UserRankingResult(dataLoaded, loadStatus));
        }
    }
}
