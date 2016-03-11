using System;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class AttachmentViewModel : MvxNotifyPropertyChanged
    {
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireAttachmentStorage attachmentStorage;

        //private AttachmentMetadata attachmentMetadata;
        private Attachment attachment;

        public AttachmentViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireAttachmentStorage attachmentStorage)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.attachmentStorage = attachmentStorage;
        }


        public async void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            var attachment = questionnaire.GetAttachmentIdForEntity(entityIdentity.Id);

            if (attachment != null)
            {
                //this.attachmentMetadata = await this.attachmentStorage.GetAttachmentAsync(attachmentId);
                this.AttachmentContent = await this.attachmentStorage.GetAttachmentContentAsync(attachment.AttachmentId.FormatGuid());
            }
        }

        public bool IsImage
        {
            get { return this.attachment != null /*&& this.attachment.FileName.Contains(".jpg")*/; }
        }

        public byte[] AttachmentContent { get; set; }
    }
}