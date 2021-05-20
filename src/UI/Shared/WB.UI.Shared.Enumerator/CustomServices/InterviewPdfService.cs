using System;
using Android.Content;
using Android.Widget;
using MvvmCross.Platforms.Android;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomServices
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

        public void Open(string interviewId, Identity identity)
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);
            var attachment = questionnaire.GetAttachmentForEntity(identity.Id);

            var attachmentContentPath = this.attachmentStorage.GetFileCacheLocation(attachment.ContentId);

            OpenPdfInExternalApp(attachmentContentPath);
        }

        public void OpenAttachment(string interviewId, Guid attachmentId)
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);
            var attachment = questionnaire.GetAttachmentById(attachmentId);
            var attachmentContentPath = this.attachmentStorage.GetFileCacheLocation(attachment.ContentId);
            OpenPdfInExternalApp(attachmentContentPath);
        }

        private void OpenPdfInExternalApp(string attachmentContentPath)
        {
            externalAppLauncher.OpenPdf(attachmentContentPath);
        }
    }
}