namespace WB.Core.Infrastructure.Backup
{
    public interface IBackupable
    {
        string GetPathToBakupFile();
        void RestoreFromBakupFolder(string path);
    }
}