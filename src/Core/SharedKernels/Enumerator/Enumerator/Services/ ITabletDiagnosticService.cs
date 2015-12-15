namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface ITabletDiagnosticService
    {
        void LaunchShareAction(string title, string info);
        void UpdateTheApp(string url);
        void RestartTheApp();
    }
}