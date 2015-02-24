namespace WB.Core.Synchronization.SyncStorage
{
    public class InterviewSyncPackageContent : ISyncPackage
    {
        public InterviewSyncPackageContent()
        {
        }

        public InterviewSyncPackageContent(string content, string metaInfo)
        {
            Content = content;
            MetaInfo = metaInfo;
        }

        public string PackageId { get; set; }
        public string Content { get; set; }
        public string MetaInfo { get; set; }
    }
}
