using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.CommandBus;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.WebTester.Services.Implementation
{
    public class EvictionService : IDisposable
    {
        private readonly IDisposable eviction;
        private readonly IWebInterviewNotificationService webInterviewNotification;
        private readonly IAppdomainsPerInterviewManager appdomainsPerInterviewManager;
        private readonly IQuestionnaireImportService questionnaireImportService;
        private readonly ICacheStorage<List<ICommand>, Guid> executedCommandsStorage;

        public EvictionService(IEvictionObservable eviction,
            IWebInterviewNotificationService webInterviewNotification,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager,
            IQuestionnaireImportService questionnaireImportService, 
            ICacheStorage<List<ICommand>, Guid> executedCommandsStorage)
        {
            this.eviction = eviction.Subscribe(Evict);
            this.webInterviewNotification = webInterviewNotification;
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
            this.questionnaireImportService = questionnaireImportService;
            this.executedCommandsStorage = executedCommandsStorage;
        }

        private void Evict(Guid interviewId)
        {
            webInterviewNotification.ShutDownInterview(interviewId);
            appdomainsPerInterviewManager.TearDown(interviewId);
            questionnaireImportService.RemoveQuestionnaire(interviewId);
            executedCommandsStorage.Remove(interviewId);
        }

        public void Dispose()
        {
            eviction.Dispose();
        }
    }
}