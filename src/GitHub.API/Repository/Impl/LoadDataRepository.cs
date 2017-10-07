using GitHub.API.Model;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace GitHub.API.Repository.Impl
{
    public class LoadDataRepository : ILoadDataRepository
    {

        private IMemoryCache _memoryCache;

        public LoadDataRepository(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public List<RankingUser> GetDataLoaded(string location)
        {
            List<RankingUser> dataLoaded = (_memoryCache.TryGetValue<List<RankingUser>>(location, out var toReturn) ? toReturn : new List<RankingUser>());
            return new List<RankingUser>(dataLoaded);
        }

        public List<RankingUser> GetDataLoadedMutable(string location)
        {
            return (_memoryCache.TryGetValue<List<RankingUser>>(location, out var toReturn) ? toReturn : new List<RankingUser>());
        }

        public void SetData(IReadOnlyList<RankingUser> users, string location)
        {
            if (users == null || (users != null && users.Count == 0))
                return;

            if (_memoryCache.Get(location) == null)
                _memoryCache.Set(location, new List<RankingUser>());

            if (_memoryCache.TryGetValue<List<RankingUser>>(location, out var usersMemory))
                usersMemory.AddRange(users);
        }
    }
}
