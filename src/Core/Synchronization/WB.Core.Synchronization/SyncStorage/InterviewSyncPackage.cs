using System;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.SyncStorage
{
    public class InterviewResponsible : IView
    {
        public Guid InterviewId { get; set; }

        public Guid UserId { get; set; }
    }

    public class InterviewSyncPackage : ISyncPackage
    {
        [Obsolete("Probably used for deserialization")]
        public InterviewSyncPackage()
        {
        }

        public InterviewSyncPackage(Guid interviewId,
            Guid questionnaireId,
            long questionnaireVersion,
            string content,
            DateTime timestamp,
            Guid? userId,
            string itemType,
            string metaInfo,
            long sortIndex)
        {
            this.InterviewId = interviewId;
            this.VersionedQuestionnaireId = string.Format("{0}_{1}", questionnaireId, questionnaireVersion);
            this.PackageId = interviewId.FormatGuid() + "$" + sortIndex;
            this.Content = content;
            this.Timestamp = timestamp;
            this.UserId = userId ?? Guid.Empty;
            this.ItemType = itemType;
            this.MetaInfo = metaInfo;
            this.SortIndex = sortIndex;
        }

        public Guid InterviewId { get; private set; }

        public string VersionedQuestionnaireId { get; private set; }

        public string PackageId { get; private set; }

        public string Content { get; private set; }

        public string MetaInfo { get; private set; }

        public DateTime Timestamp { get; private set; }

        public Guid UserId { get; private set; }

        public string ItemType { get; private set; }

        public long SortIndex { get; private set; }
    }
}