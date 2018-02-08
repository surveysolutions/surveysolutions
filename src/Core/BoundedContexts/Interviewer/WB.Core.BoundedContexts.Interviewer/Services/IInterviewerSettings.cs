using System;
using WB.Core.SharedKernels.Enumerator;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewerSettings : IEnumeratorSettings
    {
        Version GetSupportedQuestionnaireContentVersion();
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
        void SetVibrateOnError(bool vibrate);
        void SetTestingConditions(bool testingConditions);
        void SetShowLocationOnMap(bool showLocationOnMap);

        string BackupFolder { get; }
        string RestoreFolder { get; }

        string BandwidthTestUri { get; }
        string InstallationFilePath { get; }
    }
}
