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

        public QuestionnaireSyncPackageMetaInformation(Guid questionnaireId, long questionnaireVersion, long sortIndex, DateTime timestamp, string itemType)
        {
            this.PackageId = string.Format("{0}_{1}${2}", questionnaireId.FormatGuid(), questionnaireVersion, sortIndex);
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.Timestamp = timestamp;
            ItemType = itemType;
            this.SortIndex = sortIndex;
        }

        public Guid QuestionnaireId { get; private set; }

        public long QuestionnaireVersion { get; private set; }

        public string PackageId { get; private set; }

        public DateTime Timestamp { get; private set; }

        public long SortIndex { get; private set; }

        public string ItemType { get; private set; }
    }
}