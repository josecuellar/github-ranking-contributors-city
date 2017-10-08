using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace GitHub.API.Model
{
    public class LoadStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum StatusItems
        {
            FINISHED,
            RUNNING,
            CALCULATING_ORDER,
            STOPPED
        }

        public long TotalResults { get; set; }

        public long TotalResultsLoaded { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastUpdated { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? TotalOrderCalculated { get; set; } = null;

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
