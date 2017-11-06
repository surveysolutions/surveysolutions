using System;
using System.Threading;
using System.Threading.Tasks;
using Plugin.Permissions.Abstractions;
using WB.Core.BoundedContexts.Interviewer.Views;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public class MapSyncProgress
    {
        public bool IsRunning => this.Status == MapSyncStatus.Download ||
                                 this.Status == MapSyncStatus.Started;

        public string Title { get; set; }
        public int TotalMapsCount { get; set; }


        public MapSyncStatus Status { get; set; }
    }
}
