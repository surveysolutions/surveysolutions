﻿using System;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.BoundedContexts.Interviewer.Synchronization
{
    public class InterviewerUpdateAppSettings : UpdateAppSettings
    { 
        private readonly IInterviewerSettings interviewerSettings;

        public InterviewerUpdateAppSettings(int sortOrder, 
            ISynchronizationService synchronizationService,
            ILogger logger, 
            IInterviewerSettings interviewerSettings,
            ITabletDiagnosticService diagnosticService) : base(synchronizationService, 
            logger, sortOrder)
        {
            this.interviewerSettings = interviewerSettings ?? throw new ArgumentNullException(nameof(interviewerSettings));
        }
        
        protected override void UpdateNotificationsSetting(bool notificationsEnabled)
        {
            interviewerSettings.SetNotifications(notificationsEnabled);
        }
    }
}
