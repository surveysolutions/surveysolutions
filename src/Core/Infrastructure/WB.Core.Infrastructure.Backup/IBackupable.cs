namespace WB.Core.Infrastructure.Backup
{
    public interface IBackupable
    {
        string GetPathToBackupFile();
        void RestoreFromBackupFolder(string path);
    }
}