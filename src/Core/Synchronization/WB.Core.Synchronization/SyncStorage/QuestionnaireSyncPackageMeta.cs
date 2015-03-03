using System;
namespace WB.Core.Synchronization.SyncStorage
{
    public class QuestionnaireSyncPackageMeta : IOrderableSyncPackage
    {
        [Obsolete("Probably used for deserialization")]
        public QuestionnaireSyncPackageMeta()
        {
        }

        public QuestionnaireSyncPackageMeta(Guid questionnaireId, long questionnaireVersion, DateTime timestamp, string itemType, int contentSize, int metaInfoSize)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.Timestamp = timestamp;
            this.ItemType = itemType;
            this.ContentSize = contentSize;
            this.MetaInfoSize = metaInfoSize;
        }

        public Guid QuestionnaireId { get; private set; }

        public long QuestionnaireVersion { get; private set; }

        public string PackageId { get; set; }

        public DateTime Timestamp { get; private set; }

        public long SortIndex { get; set; }

        public string ItemType { get; private set; }

        public int ContentSize { get; private set; }

        public int MetaInfoSize { get; private set; }
    }
}