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

        public void AddLoaded(string location, long loaded)
        {
            var currentStatus = GetStatus(location);
            var totalLoaded = (currentStatus.TotalResultsLoaded + loaded); 
            currentStatus.TotalResultsLoaded = (totalLoaded > currentStatus.TotalResults ? currentStatus.TotalResults : totalLoaded); //;P
        }

        public void SetRunning(string location, long total)
        {
            var currentStatus = GetStatus(location);
            currentStatus.TotalResults = total;
            currentStatus.Status = StatusItems.RUNNING;
        }

        public void SetRunning(string location)
        {
            var currentStatus = GetStatus(location);
            currentStatus.Status = StatusItems.RUNNING;
        }

        public void SetCalculatingOrder(string location)
        {
            var currentStatus = GetStatus(location);
            currentStatus.Status = StatusItems.CALCULATING_ORDER;
        }

        public void SetFinished(string location, long total)
        {
            var currentStatus = GetStatus(location);
            currentStatus.TotalResults = total;
            currentStatus.TotalResultsLoaded = total;
            currentStatus.Status = StatusItems.FINISHED;
        }

    }
}
