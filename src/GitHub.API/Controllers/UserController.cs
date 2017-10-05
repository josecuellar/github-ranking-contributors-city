using GitHub.API.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GitHub.API.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {

        private ILoadDataService _service;

        public UsersController(ILoadDataService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            await _service.LoadUsersFromLocationAndPersist("Barcelona");
            return "Processed OK";
        }
    }
}
