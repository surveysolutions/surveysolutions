using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.SyncStorage
{
    [Obsolete]
    public class InterviewSyncPackageMeta : IView
    {
        public virtual string PackageId { get; set; }
        public virtual long SortIndex { get; set; }
        public virtual Guid InterviewId { get; set; }
        public virtual Guid UserId { get; set; }
        public virtual string ItemType { get; set; }
        public virtual int SerializedPackageSize { get; set; }
    }
}