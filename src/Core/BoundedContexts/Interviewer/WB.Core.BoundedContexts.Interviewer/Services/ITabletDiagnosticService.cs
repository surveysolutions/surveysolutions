using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ITabletDiagnosticService
    {
        void LaunchShareAction(string title, string info);
        Task UpdateTheApp(string url);
        void RestartTheApp();
    }
}