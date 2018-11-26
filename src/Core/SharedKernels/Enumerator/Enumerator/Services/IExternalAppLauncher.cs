namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IExternalAppLauncher
    {
        void LaunchMapsWithTargetLocation(double latitude, double longitude);
        void OpenPdf(string pathToPdfFile);
    }
}
