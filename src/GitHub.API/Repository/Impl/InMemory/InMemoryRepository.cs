using GitHub.API.Model;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GitHub.API.Repository.Impl.InMemory
{
    public class InMemoryRepository : IRepository
    {

        private IMemoryCache _memoryCache;

        public InMemoryRepository(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public ConcurrentDictionary<string, RankingUser> Get(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            return (_memoryCache.TryGetValue<ConcurrentDictionary<string, RankingUser>>(location, out var toReturn) 
                ? toReturn 
                : new ConcurrentDictionary<string, RankingUser>());
        }

        public List<RankingUser> GetOrderedByReposAndCommits(string location, int topResults)
        {

            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            try
            {
                //var dictionaryUsers = Get(location).ToDictionary(
                //    key => (key.Value.Commits + key.Value.Repositories),
                //    val => val.Value);

                return Get(location).Select(x => x.Value).ToList()
                    .OrderByDescending(x => x.Commits)
                    .ThenByDescending(x=>x.Repositories)
                    .Take(topResults)
                    .ToList();

                //return new SortedList<int, RankingUser>(dictionaryUsers)
                //    .OrderByDescending(x => x.Key).Select(x => x.Value)
                //    .Take(topResults)
                //    .ToList();
            }
            catch (Exception err)
            {
                Debug.WriteLine(err);
                return new List<RankingUser>();
            }
        }

        public int Set(ConcurrentDictionary<string, RankingUser> users, string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            if (users == null || (users != null && users.Count == 0))
            {
                Debug.WriteLine("setData with no results:" + location);
                return 0;
            }

            var usersMemory = _memoryCache.GetOrCreate(location, x => new ConcurrentDictionary<string, RankingUser>());

            int added = 0;
            foreach (var user in users)
            {
                if (usersMemory.TryAdd(user.Key, user.Value))
                    added++;
            }
            return added;
        }

        public void SetReposAndCommitsToUser(string location, string userName, int commits, int repositories)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            var data = Get(location);
            if (data.ContainsKey(userName))
            {
                data[userName].Commits = commits;
                data[userName].Repositories = repositories;
                return;
            }

            Debug.WriteLine("No contains key " + userName + " in " + location + ". No commits assigned (" + commits + ") to user");
        }
    }
}
