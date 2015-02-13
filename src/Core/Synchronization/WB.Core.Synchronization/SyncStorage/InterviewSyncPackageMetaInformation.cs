using System;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.Synchronization.SyncStorage
{
    public class InterviewSyncPackageMetaInformation : ISyncPackage
    {
        [Obsolete("Probably used for deserialization")]
        public InterviewSyncPackageMetaInformation()
        {
        }

        public InterviewSyncPackageMetaInformation(Guid interviewId,
            Guid questionnaireId,
            long questionnaireVersion,
            DateTime timestamp,
            Guid? userId,
            long sortIndex, string itemType, int contentSize, int metaInfoSize)
        {
            this.InterviewId = interviewId;
            this.VersionedQuestionnaireId = string.Format("{0}_{1}", questionnaireId, questionnaireVersion);
            this.PackageId = interviewId.FormatGuid() + "$" + sortIndex;
            this.Timestamp = timestamp;
            this.UserId = userId ?? Guid.Empty;
            this.SortIndex = sortIndex;
            ItemType = itemType;
            ContentSize = contentSize;
            MetaInfoSize = metaInfoSize;
        }

        public Guid InterviewId { get; private set; }

        public string VersionedQuestionnaireId { get; private set; }

        public string PackageId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public Guid UserId { get; private set; }

        public long SortIndex { get; private set; }

        public string ItemType { get; private set; }

        public int ContentSize { get; private set; }

        public int MetaInfoSize { get; private set; }
    }
}