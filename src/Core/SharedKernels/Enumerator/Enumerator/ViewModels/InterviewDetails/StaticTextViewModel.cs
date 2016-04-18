using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class StaticTextViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel
    {
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private static readonly Regex htmlRemovalRegex = new Regex("<.*?>");

        public AttachmentViewModel Attachment { get; set; }
        public StaticTextStateViewModel QuestionState { get; set; }

        public StaticTextViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            AttachmentViewModel attachmentViewModel,
            StaticTextStateViewModel questionState)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.Attachment = attachmentViewModel;
            this.QuestionState = questionState;
        }

        public Identity Identity => this.identity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            this.identity = entityIdentity;

            this.QuestionState.Init(interviewId, entityIdentity);
            this.RawStaticText = questionnaire.GetStaticText(entityIdentity.Id);
            this.StaticText = RemoveHtmlTags(this.RawStaticText);

            this.Attachment.Init(interviewId, entityIdentity);
        }

        private Identity identity;

        private string rawText;
        public string RawStaticText
        {
            get { return this.rawText; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.rawText, value);
            }
        }

        private string staticText;
        public string StaticText
        {
            get { return this.staticText; }
            set { this.RaiseAndSetIfChanged(ref this.staticText, value); }
        }

        private static string RemoveHtmlTags(string rawText)
        {
            return htmlRemovalRegex.Replace(rawText, string.Empty);
        }
    }
}