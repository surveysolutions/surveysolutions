using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewerSettings : IEnumeratorSettings
    {
        string GetDeviceId();
        string GetApplicationVersionName();
        string GetDeviceTechnicalInformation();
        int GetApplicationVersionCode();
        Task SetEndpointAsync(string endpoint);
        Task SetHttpResponseTimeoutAsync(int timeout);
        Task SetGpsResponseTimeoutAsync(int timeout);
        Task SetCommunicationBufferSize(int bufferSize);
        
        string BackupFolder { get; }
        string RestoreFolder { get; }
        string CrushFilePath { get; }
    }
}
