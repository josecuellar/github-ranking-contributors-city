using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GitHub.API.Repository;
using Octokit;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace GitHub.API.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {

        private IUserRepository _userRepository;
        private IMemoryCache _memoryCache;

        public UsersController(IUserRepository userRepository, IMemoryCache memoryCache)
        {
            _userRepository = userRepository;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<IReadOnlyList<Octokit.User>> Get()
        {

            try
            {
                SearchUsersResult users = await _userRepository.GetUsersWithMoreRepositoriesFromLocation("Barcelona", 1);

                _memoryCache.Set<List<Octokit.User>>("UsersTable", (new List<Octokit.User>(users.Items)));

                int numPages = users.TotalCount / 100;

                for (int i = 2; i <= numPages; i++)
                {
                    var usersLoaded = (await _userRepository.GetUsersWithMoreRepositoriesFromLocation("Barcelona", i));
                    if (usersLoaded != null && usersLoaded.Items != null && usersLoaded.Items.Count > 0)
                    {
                        var listUsers = new List<Octokit.User>(usersLoaded.Items);
                        if (listUsers != null)
                            _memoryCache.Get<List<Octokit.User>>("UsersTable").AddRange(listUsers);
                    }
                }

                return null;
            }
            catch (Exception err)
            {

                return null;                    
            }

        }
    }
}
