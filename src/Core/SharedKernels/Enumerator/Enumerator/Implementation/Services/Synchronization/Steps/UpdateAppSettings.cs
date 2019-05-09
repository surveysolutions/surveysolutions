﻿using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public abstract class UpdateAppSettings : SynchronizationStep
    {
        protected UpdateAppSettings(ISynchronizationService synchronizationService, ILogger logger, 
            int sortOrder) : base(sortOrder, synchronizationService, logger)
        {
        }

        public override async Task ExecuteAsync()
        {
            var notificationsSetting = await this.synchronizationService.AreNotificationsEnabledAsync(Context.CancellationToken);
            UpdateNotificationsSetting(notificationsSetting);
        }

        protected abstract void UpdateNotificationsSetting(bool notificationsEnabled);
    }
}
