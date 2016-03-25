using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using Nito.AsyncEx;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class StaticTextViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        public AttachmentViewModel Attachment { get; set; }

        public StaticTextViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            AttachmentViewModel attachmentViewModel)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.Attachment = attachmentViewModel;
        }

        public Identity Identity => this.questionIdentity;

        public async Task InitAsync(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            this.questionIdentity = entityIdentity;
            this.StaticText = questionnaire.GetStaticText(entityIdentity.Id);

            await this.Attachment.InitAsync(interviewId, entityIdentity);
        }

        private string staticText;
        private Identity questionIdentity;

        public string StaticText
        {
            get { return this.staticText; }
            set { this.staticText = value; this.RaisePropertyChanged(); }
        }


    }
}