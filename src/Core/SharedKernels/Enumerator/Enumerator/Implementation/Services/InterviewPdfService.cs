using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class InterviewPdfService : IInterviewPdfService
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAttachmentContentStorage attachmentStorage;
        private readonly IExternalAppLauncher externalAppLauncher;

        public InterviewPdfService(IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IAttachmentContentStorage attachmentStorage,
            IExternalAppLauncher externalAppLauncher)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.attachmentStorage = attachmentStorage;
            this.externalAppLauncher = externalAppLauncher;
        }

        public async Task OpenAsync(string interviewId, Identity identity)
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);
            var attachment = questionnaire.GetAttachmentForEntity(identity.Id);

            var attachmentContentPath = await this.attachmentStorage.GetFileCacheLocationAsync(attachment.ContentId);

            OpenPdfInExternalApp(attachmentContentPath);
        }

        public async Task OpenAttachmentAsync(string interviewId, Guid attachmentId)
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);
            var attachment = questionnaire.GetAttachmentById(attachmentId);
            var attachmentContentPath = await this.attachmentStorage.GetFileCacheLocationAsync(attachment.ContentId);
            OpenPdfInExternalApp(attachmentContentPath);
        }

        private void OpenPdfInExternalApp(string attachmentContentPath)
        {
            externalAppLauncher.OpenPdf(attachmentContentPath);
        }
    }
}