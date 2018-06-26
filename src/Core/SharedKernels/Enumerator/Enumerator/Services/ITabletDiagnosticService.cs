using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface ITabletDiagnosticService
    {
        void LaunchShareAction(string title, string info);
        Task UpdateTheApp(CancellationToken cancellationToken, bool continueIfNoPatch, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged = null);
        void RestartTheApp();
    }
}
