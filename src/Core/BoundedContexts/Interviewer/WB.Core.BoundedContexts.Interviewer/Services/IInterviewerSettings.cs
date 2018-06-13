using System;
using WB.Core.SharedKernels.Enumerator;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewerSettings : IEnumeratorSettings, IDeviceSettings
    {
        Version GetSupportedQuestionnaireContentVersion();
        void SetVibrateOnError(bool vibrate);
        void SetShowLocationOnMap(bool showLocationOnMap);
    }
}
