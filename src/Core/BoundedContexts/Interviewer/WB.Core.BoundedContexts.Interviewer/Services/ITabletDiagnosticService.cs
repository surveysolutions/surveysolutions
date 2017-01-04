using System;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ITabletDiagnosticService
    {
        void LaunchShareAction(string title, string info);
        Task UpdateTheApp(string url, CancellationToken cancellationToken, TimeSpan timeout);
        void RestartTheApp();
    }
}