using System;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.WebInterview;
using WB.UI.WebTester.Controllers;

namespace WB.UI.WebTester.Services.Implementation
{
    public class EvictionService : IDisposable
    {
        private readonly IDisposable eviction;
        private readonly IWebInterviewNotificationService webInterviewNotification;
        private readonly IAppdomainsPerInterviewManager appdomainsPerInterviewManager;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ITranslationManagementService translationStorage;
        private readonly ICacheStorage<MultimediaFile, string> multimediaStorage;
        private readonly IPlainStorageAccessor<QuestionnaireAttachment> attachmentsStorage;

        public EvictionService(IEvictionObservable eviction,
            IWebInterviewNotificationService webInterviewNotification,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager,
            IQuestionnaireStorage questionnaireStorage,
            ITranslationManagementService translationStorage,
            ICacheStorage<MultimediaFile, string> multimediaStorage,
            IPlainStorageAccessor<QuestionnaireAttachment> attachmentsStorage)
        {
            this.eviction = eviction.Subscribe(Evict);
            this.webInterviewNotification = webInterviewNotification;
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
            this.questionnaireStorage = questionnaireStorage;
            this.translationStorage = translationStorage;
            this.multimediaStorage = multimediaStorage;
            this.attachmentsStorage = attachmentsStorage;
        }

        private void Evict(Guid interviewId)
        {
            appdomainsPerInterviewManager.TearDown(interviewId);

            var questionnaireId = new QuestionnaireIdentity(interviewId, 1);
            var questionnaire = questionnaireStorage.GetQuestionnaireDocument(questionnaireId);
            questionnaireStorage.DeleteQuestionnaireDocument(interviewId, 1);
            translationStorage.Delete(questionnaireId);
            multimediaStorage.RemoveArea(interviewId);

            foreach (var attachment in questionnaire.Attachments)
            {
                attachmentsStorage.Remove(attachment.ContentId);
            }

            webInterviewNotification.FinishInterview(interviewId);
        }

        public void Dispose()
        {
            eviction.Dispose();
        }
    }
}