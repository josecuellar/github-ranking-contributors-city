using GitHub.API.Model;
using Microsoft.Extensions.Caching.Memory;
using static GitHub.API.Model.LoadStatus;

namespace GitHub.API.Service.Impl
{
    public class StatusLoadDataService : IStatusLoadDataService
    {

        private IMemoryCache _memoryCache;

        public StatusLoadDataService(
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public LoadStatus GetStatus(string location)
        {
            if (!_memoryCache.TryGetValue<LoadStatus>("Status" + location, out var toReturn))
                _memoryCache.Set("Status" + location, new LoadStatus());

            return _memoryCache.Get<LoadStatus>("Status" + location);
        }

        public void SetStatus(string location, StatusItems status, long totalResultsLoaded, long totalResults)
        {
            var currentStatus = GetStatus(location);
            currentStatus.Status = status;
            currentStatus.TotalResults = totalResults;
            currentStatus.TotalResultsLoaded = totalResultsLoaded;
        }

        public void SetStatus(string location, StatusItems status, long totalResultsLoaded)
        {
            var currentStatus = GetStatus(location);
            currentStatus.Status = status;
            currentStatus.TotalResultsLoaded = totalResultsLoaded;
        }
    }
}
