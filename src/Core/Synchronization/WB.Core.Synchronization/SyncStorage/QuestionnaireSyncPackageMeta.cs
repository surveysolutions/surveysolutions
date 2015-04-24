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

        public virtual Guid QuestionnaireId { get; protected set; }

        public virtual long QuestionnaireVersion { get; protected set; }

        public virtual string PackageId { get; set; }

        public virtual DateTime Timestamp { get; protected set; }

        public virtual long SortIndex { get; set; }

        public virtual string ItemType { get; protected set; }

        public virtual int ContentSize { get; protected set; }

        public virtual int MetaInfoSize { get; protected set; }
    }
}