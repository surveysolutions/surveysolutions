using System;
using System.Linq;
using System.Text.RegularExpressions;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class DynamicTextViewModel : MvxNotifyPropertyChanged,
        IViewModelEventHandler<SubstitutionTitlesChanged>,
        IViewModelEventHandler<RosterInstancesTitleChanged>,
        IDisposable
    {
        private static readonly Regex HtmlRemovalRegex = new Regex(Constants.HtmlRemovalPattern);

        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public DynamicTextViewModel(
            IViewModelEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository,
            ISubstitutionService substitutionService,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.eventRegistry = eventRegistry;
            this.interviewRepository = interviewRepository;
            this.substitutionService = substitutionService;
            this.questionnaireStorage = questionnaireStorage;
        }

        private string interviewId;
        protected Identity identity;
        private bool shouldAppendRosterTitle;

        private bool isInstructions = false;

        public void InitAsStatic(string textWithoutSubstitutions, Identity entityId = null)
        {
            if (textWithoutSubstitutions == null) throw new ArgumentNullException(nameof(textWithoutSubstitutions));

            this.HtmlText = textWithoutSubstitutions;
            this.PlainText = textWithoutSubstitutions;
            this.identity = entityId;
        }

        public void InitAsInstructions(string interviewId, Identity entityIdentity)
        {
            isInstructions = true;
            this.Init(interviewId, entityIdentity);
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.interviewId = interviewId;
            this.identity = entityIdentity;

            var interview = this.interviewRepository.GetOrThrow(this.interviewId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);

            this.shouldAppendRosterTitle = questionnaire.IsRosterGroup(this.identity.Id) &&
                !questionnaire.HasCustomRosterTitle(this.identity.Id);

            this.UpdateText();

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private string htmlText;
        public string HtmlText
        {
            get { return this.htmlText; }
            set { this.RaiseAndSetIfChanged(ref this.htmlText, value); }
        }

        private string plainText;
        public string PlainText
        {
            get { return this.plainText; }
            set { this.RaiseAndSetIfChanged(ref this.plainText, value); }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
        }

        public void Handle(SubstitutionTitlesChanged @event)
        {
            bool shouldUpdateTexts =
                @event.Questions.Contains(this.identity) ||
                @event.StaticTexts.Contains(this.identity) ||
                @event.Groups.Contains(this.identity);

            if (!shouldUpdateTexts) return;

            this.UpdateText();
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            if (!this.shouldAppendRosterTitle) return;

            if (!@event.ChangedInstances.Any(x => x.RosterInstance.GetIdentity().Equals(this.identity)))
                return;

            this.UpdateText();
        }

        private void UpdateText()
        {
            var interview = this.interviewRepository.GetOrThrow(this.interviewId);

            if (isInstructions)
            {
                this.HtmlText = (interview.GetBrowserReadyInstructionsHtml(this.identity) ?? "");
                    //.Replace(Environment.NewLine, "<br/>");
            }
            else
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);

                var titleText = questionnaire.IsVariable(this.identity.Id)
                    ? questionnaire.GetVariableLabel(this.identity.Id)
                    : interview.GetBrowserReadyTitleHtml(this.identity);

                titleText ??= "";

                this.HtmlText = this.shouldAppendRosterTitle
                    ? $"{titleText} - {interview.GetRosterTitle(this.identity) ?? this.substitutionService.DefaultSubstitutionText}"
                    : titleText;
            }

            this.PlainText = RemoveHtmlTags(this.HtmlText);
        }

        private static string RemoveHtmlTags(string rawText) => HtmlRemovalRegex.Replace(rawText, string.Empty);
    }
}
