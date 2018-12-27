﻿using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Services.Synchronization
{
    public class SyncProgressInfo
    {
        public SyncProgressInfo()
        {
            this.Statistics = new SynchronizationStatistics();
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public SyncStage? Stage { get; set; }
        public Dictionary<string, string> StageExtraInfo = new Dictionary<string, string>();
        public SynchronizationStatus Status { get; set; }
        public SynchronizationStatistics Statistics { get; set; }
        public bool UserIsLinkedToAnotherDevice { get; set; }
        public bool IsApplicationUpdateRequired { get; set; }

        public TransferProgress TransferProgress { get; set; }

        public bool HasErrors => this.Statistics.FailedToUploadInterviewsCount != 0 || 
                                 this.Statistics.FailedToCreateInterviewsCount != 0;

        public bool IsRunning => this.Status == SynchronizationStatus.Download || 
                                 this.Status == SynchronizationStatus.Started ||
                                 this.Status == SynchronizationStatus.Upload;
    }
}

