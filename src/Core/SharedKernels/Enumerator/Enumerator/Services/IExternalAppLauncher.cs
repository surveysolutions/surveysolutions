namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IExternalAppLauncher
    {
        void LaunchMapsWithTargetLocation(double latitude, double longitude);
        void LaunchShareAction(string title, string info);
        void UpdateTheApp(string url);
    }
}