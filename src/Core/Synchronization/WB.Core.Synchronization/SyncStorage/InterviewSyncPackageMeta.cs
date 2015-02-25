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

        public string PackageId { get; set; }

        public long SortIndex { get; set; }

        public Guid InterviewId { get; private set; }

        public string VersionedQuestionnaireId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public Guid UserId { get; private set; }

        public string ItemType { get; private set; }

        public int ContentSize { get; private set; }

        public int MetaInfoSize { get; private set; }
    }
}