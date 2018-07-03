using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IDeviceSettings : IRestServiceSettings
    {
        string GetDeviceId();
        string GetApplicationVersionName();
        string GetDeviceTechnicalInformation();
        int GetApplicationVersionCode();
        void SetEndpoint(string endpoint);
        void SetHttpResponseTimeout(int timeout);
        void SetGpsResponseTimeout(int timeout);
        void SetCommunicationBufferSize(int bufferSize);
        void SetGpsDesiredAccuracy(double value);
        void SetEventChunkSize(int eventChunkSize);
        string BackupFolder { get; }
        string RestoreFolder { get; }

        string BandwidthTestUri { get; }
        string InstallationFilePath { get; }
    }
}