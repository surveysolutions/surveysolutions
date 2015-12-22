using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IBackupRestoreService
    {
        Task<byte[]> GetSystemBackupAsync();
        Task<string> BackupAsync(string backupToFolderPath);
        Task RestoreAsync(string backupFilePath);
    }
}