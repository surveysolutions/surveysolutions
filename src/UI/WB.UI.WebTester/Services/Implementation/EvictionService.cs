using System;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.WebTester.Services.Implementation
{
    public class EvictionService : IDisposable
    {
        private readonly IDisposable eviction;
        private readonly IWebInterviewNotificationService webInterviewNotification;
        private readonly IAppdomainsPerInterviewManager appdomainsPerInterviewManager;
        private readonly IQuestionnaireImportService questionnaireImportService;

        public EvictionService(IEvictionObservable eviction,
            IWebInterviewNotificationService webInterviewNotification,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager,
            IQuestionnaireImportService questionnaireImportService)
        {
            this.eviction = eviction.Subscribe(Evict);
            this.webInterviewNotification = webInterviewNotification;
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
            this.questionnaireImportService = questionnaireImportService;
        }

        private void Evict(Guid interviewId)
        {
            webInterviewNotification.ShutDownInterview(interviewId);
            appdomainsPerInterviewManager.TearDown(interviewId);
            questionnaireImportService.RemoveQuestionnaire(interviewId);
        }

        public void Dispose()
        {
            eviction.Dispose();
        }
    }
}