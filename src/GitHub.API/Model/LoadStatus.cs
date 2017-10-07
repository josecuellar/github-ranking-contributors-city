using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GitHub.API.Model
{
    public class LoadStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum StatusItems
        {
            FINISHED,
            RUNNING,
            STOPPED
        }

        public long TotalResults { get; set; }

        public long TotalResultsLoaded { get; set; }

        public StatusItems Status { get; set; }

        public LoadStatus(long totalResults, long totalResultsLoaded, StatusItems status)
        {
            Status = status;
            TotalResults = TotalResults;
            TotalResultsLoaded = totalResultsLoaded;
        }
        public LoadStatus()
        {
            Status = StatusItems.STOPPED;
            TotalResults = 0;
            TotalResultsLoaded = 0;
        }
    }
}
