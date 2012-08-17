using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Synchronization.Core.Interface;

namespace Synchronization.Core.SynchronizationFlow
{
    public struct SyncStatus : ISyncProgressStatus
    {
        int progressPercents;
        bool isCanceled;
        bool isError;

        public SyncStatus(int progress, bool error, bool cancel)
        {
            this.isCanceled = cancel;
            this.progressPercents = progress;
            this.isError = error;
        }

        public int ProgressPercents { get { return this.progressPercents; } }
        public bool IsError { get { return this.isError; } }
        public bool IsCanceled { get { return this.isCanceled; } }
    }
}
