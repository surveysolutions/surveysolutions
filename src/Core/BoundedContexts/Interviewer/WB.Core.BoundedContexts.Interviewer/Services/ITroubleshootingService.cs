using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ITroubleshootingService
    {
        Task<byte[]> GetSystemBackupAsync();
        Task BackupAsync(string backupFilePath);
        Task RestoreAsync(string backupFilePath);
    }
}