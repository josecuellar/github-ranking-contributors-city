using GitHub.API.Model;
using Microsoft.Extensions.Caching.Memory;
using System;
using static GitHub.API.Model.LoadStatus;

namespace GitHub.API.Repository.Impl.InMemory
{
    public class InMemoryStatusRepository : IStatusRepository
    {

        private IMemoryCache _memoryCache;

        private const string _PREFIX_KEY_STATUS = "Status_";

        public InMemoryStatusRepository(
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public LoadStatus Get(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            if (!_memoryCache.TryGetValue<LoadStatus>(_PREFIX_KEY_STATUS + location, out var toReturn))
                _memoryCache.Set(_PREFIX_KEY_STATUS + location, new LoadStatus());

            return _memoryCache.Get<LoadStatus>(_PREFIX_KEY_STATUS + location);
        }

        public void AddLoaded(string location, long loaded)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            if (loaded < 0)
                throw new ArgumentNullException("partial loaded is not valid");

            var currentStatus = Get(location);
            currentStatus.TotalResultsLoaded = (currentStatus.TotalResultsLoaded + loaded);
        }

        public void SetOrderCalculated(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            var currentStatus = Get(location);
            currentStatus.TotalOrderCalculated = (currentStatus.TotalOrderCalculated + 1);
        }

        public void SetRunning(string location, long total)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            if (total < 0)
                throw new ArgumentNullException("total is not valid");

            var currentStatus = Get(location);
            currentStatus.TotalResults = total;
            currentStatus.Status = StatusItems.RUNNING;
        }

        public void SetRunning(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            var currentStatus = Get(location);
            currentStatus.Status = StatusItems.RUNNING;
        }

        public void SetCalculatingOrder(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            var currentStatus = Get(location);
            currentStatus.Status = StatusItems.CALCULATING_ORDER;
            currentStatus.TotalOrderCalculated = 0;
        }

        public void SetFinished(string location, long total)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location is mandatory");

            if (total < 0)
                throw new ArgumentNullException("total is not valid");

            var currentStatus = Get(location);
            currentStatus.TotalResults = total;
            currentStatus.TotalResultsLoaded = total;
            currentStatus.Status = StatusItems.FINISHED;
            currentStatus.LastUpdated = DateTime.Now;
        }

    }
}
