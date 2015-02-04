using System;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.Synchronization.SyncStorage
{
    public class QuestionnaireSyncPackage : IIndexedView
    {
        [Obsolete("Probably used for deserialization")]
        public QuestionnaireSyncPackage()
        {
        }

        public QuestionnaireSyncPackage(Guid questionnaireId, long questionnaireVersion, string itemType, string content, string metaInfo, int sortIndex, DateTime timestamp)
        {
            this.PackageId = string.Format("{0}_{1}${2}", questionnaireId.FormatGuid(), questionnaireVersion, sortIndex);
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.Content = content;
            this.Timestamp = timestamp;
            this.ItemType = itemType;
            this.MetaInfo = metaInfo;
            this.SortIndex = sortIndex;
        }

        public Guid QuestionnaireId { get; private set; }

        public long QuestionnaireVersion { get; private set; }

        public string PackageId { get; private set; }

        public string Content { get; private set; }

        public DateTime Timestamp { get; private set; }

        public string ItemType { get; private set; }

        public string MetaInfo { get; private set; }

        public int SortIndex { get; private set; }
    }
}