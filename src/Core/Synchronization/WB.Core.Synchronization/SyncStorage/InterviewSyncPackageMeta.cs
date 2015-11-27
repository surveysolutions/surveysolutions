using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.SyncStorage
{
    [Obsolete]
    public class InterviewSyncPackageMeta : IView
    {
        //DO not delete - is used for deserialization
        public InterviewSyncPackageMeta()
        {
        }

        public InterviewSyncPackageMeta(Guid interviewId,
            Guid? userId, 
            string itemType, 
            int serializedPackageSize)
        {
            this.InterviewId = interviewId;
            this.UserId = userId ?? Guid.Empty;
            this.ItemType = itemType;
            this.SerializedPackageSize = serializedPackageSize;
        }

        public virtual string PackageId { get; set; }
        public virtual long SortIndex { get; set; }
        public virtual Guid InterviewId { get; protected set; }
        public virtual Guid UserId { get; protected set; }
        public virtual string ItemType { get; protected set; }
        public virtual int SerializedPackageSize { get; protected set; }
    }
}