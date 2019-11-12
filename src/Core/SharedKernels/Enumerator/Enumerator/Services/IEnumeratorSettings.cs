﻿using System;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public enum EnumeratorApplicationType
    {
        WithMaps = 1,
        WithoutMaps
    }

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
        EnumeratorApplicationType ApplicationType { get; }
        bool Encrypted { get; }
        void SetEncrypted(bool encrypted);
        bool NotificationsEnabled { get; }
        void SetNotifications(bool notificationsEnabled);
        DateTime? LastSync { get; }
        bool? LastSyncSucceeded { get; }
        void MarkSyncStart();
        void MarkSyncSucceeded();

        bool DashboardViewsUpdated { get; }
        void SetDashboardViewsUpdated(bool updated);
    }
}
