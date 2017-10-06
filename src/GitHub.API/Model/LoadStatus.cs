using System;

namespace GitHub.API.Model
{
    public class LoadStatus
    {
        public enum StatusItems
        {
            FINISHED,
            RUNNING,
            STOPPED
        }

        public DateTime LoadedUntil { get; private set; }

        public StatusItems Status { get; private set; }

        public LoadStatus(DateTime loadedUntil, StatusItems status)
        {
            LoadedUntil = loadedUntil;
            Status = status;
        }

        public LoadStatus()
        {
            LoadedUntil = DateTime.MinValue;
            Status =  StatusItems.STOPPED;
        }
    }
}
