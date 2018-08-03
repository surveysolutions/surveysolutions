using System;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IEnumeratorSettings : IRestServiceSettings
    {
        int EventChunkSize { get; }
        Version GetSupportedQuestionnaireContentVersion();

        int GpsReceiveTimeoutSec { get; }
        double GpsDesiredAccuracy { get; }
        bool VibrateOnError { get; }
        bool ShowVariables { get; }
        bool ShowLocationOnMap { get; }
        bool ShowAnswerTime { get; }
        long? LastHqSyncTimestamp { get; }
        void SetLastHqSyncTimestamp(long? lastHqSyncTimestamp);
    }
}
