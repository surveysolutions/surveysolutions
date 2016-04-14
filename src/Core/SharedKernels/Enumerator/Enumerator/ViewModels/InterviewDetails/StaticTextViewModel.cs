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

        public async Task InitAsync(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            this.identity = entityIdentity;

            this.QuestionState.Init(interviewId, entityIdentity);
            this.rawText = questionnaire.GetStaticText(entityIdentity.Id);

            this.SetStaticText();

            this.QuestionState.Enablement.EntityDisabled += (s,a) => this.SetStaticText();
            this.QuestionState.Enablement.EntityEnabled += (s, a) => this.SetStaticText();
            this.QuestionState.Validity.BecameInvalid += (s, a) => this.SetStaticText();
            this.QuestionState.Validity.BecameValid += (s, a) => this.SetStaticText();

            await this.Attachment.InitAsync(interviewId, entityIdentity);
        }

        private void SetStaticText()
        {
            if (ShouldStipHtml)
            {
                this.StaticText = RemoveHtmlTags(this.rawText);
            }
            else
            {
                this.StaticText = this.rawText;
            }
        }

        private bool ShouldStipHtml => !this.QuestionState.Enablement.Enabled || this.QuestionState.Validity.IsInvalid;

        private Identity identity;
        private string rawText;

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