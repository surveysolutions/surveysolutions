using System.IO;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IBackupRestoreService
    {
        Task<string> BackupAsync();
        Task<string> BackupAsync(string backupToFolderPath);
        Task RestoreAsync(string backupFilePath);
    }
}