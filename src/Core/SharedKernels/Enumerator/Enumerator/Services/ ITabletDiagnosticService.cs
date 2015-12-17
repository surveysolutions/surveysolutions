using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface ITabletDiagnosticService
    {
        void LaunchShareAction(string title, string info);
        Task UpdateTheApp(string url);
        void RestartTheApp();
    }
}