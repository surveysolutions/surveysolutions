using System;

namespace WB.Core.Synchronization.SyncStorage
{
    public class InterviewSyncPackageMeta : IOrderableSyncPackage
    {
        [Obsolete("Probably used for deserialization")]
        public InterviewSyncPackageMeta()
        {
        }

        public InterviewSyncPackageMeta(Guid interviewId,
            Guid questionnaireId,
            long questionnaireVersion,
            DateTime timestamp,
            Guid? userId, 
            string itemType, 
            int contentSize, 
            int metaInfoSize)
        {
            this.InterviewId = interviewId;
            this.VersionedQuestionnaireId = string.Format("{0}_{1}", questionnaireId, questionnaireVersion);
            this.Timestamp = timestamp;
            this.UserId = userId ?? Guid.Empty;
            this.ItemType = itemType;
            this.ContentSize = contentSize;
            this.MetaInfoSize = metaInfoSize;
        }

        public virtual string PackageId { get; set; }
        public virtual long SortIndex { get; set; }
        public virtual Guid InterviewId { get; private set; }
        public virtual string VersionedQuestionnaireId { get; private set; }
        public virtual DateTime Timestamp { get; private set; }
        public virtual Guid UserId { get; private set; }
        public virtual string ItemType { get; private set; }
        public virtual int ContentSize { get; private set; }
        public virtual int MetaInfoSize { get; private set; }
    }
}