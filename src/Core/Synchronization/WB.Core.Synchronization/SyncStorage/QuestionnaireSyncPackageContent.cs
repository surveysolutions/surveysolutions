namespace WB.Core.Synchronization.SyncStorage
{
    public class QuestionnaireSyncPackageContent : ISyncPackage
    {
        public QuestionnaireSyncPackageContent()
        {
        }

        public QuestionnaireSyncPackageContent(string content, string metaInfo)
        {
            Content = content;
            MetaInfo = metaInfo;
        }

        public string PackageId { get;  set; }

        public string Content { get;  set; }

        public string MetaInfo { get;  set; }
    }
}
