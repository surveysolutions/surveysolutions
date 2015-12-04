using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ITroubleshootingService
    {
        byte[] GetSystemBackup();
        Task BackupAsync(string backupToFolderPath);
        void Restore(string backupFilePath);
    }
}