using System;
using System.Linq;
using System.Text.RegularExpressions;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class StaticTextViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventHandler<SubstitutionTitlesChanged>,
        ILiteEventHandler<VariablesChanged>,
        IDisposable
    {
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry registry;
        private readonly SubstitutionViewModel substitutionViewModel;
        private static readonly Regex htmlRemovalRegex = new Regex("<.*?>");

        public AttachmentViewModel Attachment { get; set; }
        public StaticTextStateViewModel QuestionState { get; set; }

        public StaticTextViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            AttachmentViewModel attachmentViewModel,
            StaticTextStateViewModel questionState,
            ILiteEventRegistry registry,
            SubstitutionViewModel substitutionViewModel)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.registry = registry;
            this.substitutionViewModel = substitutionViewModel;
            this.Attachment = attachmentViewModel;
            this.QuestionState = questionState;
        }

        public Identity Identity => this.identity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.interviewId = interviewId;

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            this.identity = entityIdentity;

            this.QuestionState.Init(interviewId, entityIdentity);
            
            this.substitutionViewModel.Init(interviewId, entityIdentity, questionnaire.GetStaticText(entityIdentity.Id));

            this.UpdateUITexts();

            this.Attachment.Init(interviewId, entityIdentity);
            this.registry.Subscribe(this, interviewId);
        }

        private Identity identity;
        private string interviewId;

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

        public void Dispose()
        {
            this.registry.Unsubscribe(this, this.interviewId);
            this.QuestionState.Dispose();
        }

        public void Handle(VariablesChanged @event)
        {
            if (!this.substitutionViewModel.HasVariablesInText(
                @event.ChangedVariables.Select(variable => variable.Identity)))
                return;

            this.UpdateUITexts();
        }

        public void Handle(SubstitutionTitlesChanged @event)
        {
            if (@event.StaticTexts.Length > 0)
            {
                this.UpdateUITexts();
            }
        }

        private void UpdateUITexts()
        {
            this.RawStaticText = this.substitutionViewModel.ReplaceSubstitutions();
            this.StaticText = RemoveHtmlTags(this.RawStaticText);
        }
    }
}