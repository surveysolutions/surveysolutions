namespace WB.Core.Synchronization.SyncStorage
{
    public class UserSyncPackageContent : ISyncPackage
    {
        public UserSyncPackageContent()
        {
        }

        public UserSyncPackageContent(string content)
        {
            this.Content = content;
        }

        public string PackageId { get; set; }

        public string Content { get; set; }
    }
}