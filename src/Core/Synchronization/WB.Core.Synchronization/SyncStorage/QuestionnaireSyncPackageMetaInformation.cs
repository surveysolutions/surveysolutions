using System;

using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.Synchronization.SyncStorage
{
    public class QuestionnaireSyncPackageMetaInformation : ISyncPackage
    {
        [Obsolete("Probably used for deserialization")]
        public QuestionnaireSyncPackageMetaInformation()
        {
        }

        public QuestionnaireSyncPackageMetaInformation(Guid questionnaireId, long questionnaireVersion, long sortIndex, DateTime timestamp, string itemType, int contentSize, int metaInfoSize)
        {
            this.PackageId = string.Format("{0}_{1}${2}", questionnaireId.FormatGuid(), questionnaireVersion, sortIndex);
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.Timestamp = timestamp;
            ItemType = itemType;
            ContentSize = contentSize;
            MetaInfoSize = metaInfoSize;
            this.SortIndex = sortIndex;
        }

        public Guid QuestionnaireId { get; private set; }

        public long QuestionnaireVersion { get; private set; }

        public string PackageId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public long SortIndex { get; private set; }

        public string ItemType { get; private set; }

        public int ContentSize { get; private set; }

        public int MetaInfoSize { get; private set; }
    }
}