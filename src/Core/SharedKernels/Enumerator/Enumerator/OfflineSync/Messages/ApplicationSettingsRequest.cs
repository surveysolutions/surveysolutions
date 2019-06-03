using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class ApplicationSettingsRequest : ICommunicationMessage
    {
    }

    public class ApplicationSettingsResponse : ICommunicationMessage
    {
        public bool NotificationsEnabled { get; set; }
    }
}
