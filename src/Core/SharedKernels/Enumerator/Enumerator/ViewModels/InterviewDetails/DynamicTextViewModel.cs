using System;
using System.Linq;
using System.Text.RegularExpressions;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class DynamicTextViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<SubstitutionTitlesChanged>,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        IDisposable
    {
        private static readonly Regex HtmlRemovalRegex = new Regex(Constants.HtmlRemovalPattern);

        private readonly ILiteEventRegistry eventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ISubstitutionService substitutionService;

        public DynamicTextViewModel(
            ILiteEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository,
            ISubstitutionService substitutionService)
        {
            this.eventRegistry = eventRegistry;
            this.interviewRepository = interviewRepository;
            this.substitutionService = substitutionService;
        }

        private string interviewId;
        protected Identity identity;
        private bool isRoster;

        private bool isInstructions = false;

        public void InitAsStatic(string textWithoutSubstitutions)
        {
            if (textWithoutSubstitutions == null) throw new ArgumentNullException(nameof(textWithoutSubstitutions));

            this.HtmlText = textWithoutSubstitutions;
            this.PlainText = textWithoutSubstitutions;
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

            var interview = this.interviewRepository.Get(this.interviewId);
            this.isRoster = interview.GetRoster(this.identity) != null;

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
            if (!this.isRoster) return;

            if (!@event.ChangedInstances.Any(x => x.RosterInstance.GetIdentity().Equals(this.identity)))
                return;

            this.UpdateText();
        }

        private void UpdateText()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            if (isInstructions)
            {
                this.HtmlText = interview.GetBrowserReadyInstructionsHtml(this.identity) ?? "";
            }
            else
            {
                var titleText = interview.GetBrowserReadyTitleHtml(this.identity) ?? "";

                this.HtmlText = this.isRoster
                    ? $"{titleText} - {interview.GetRosterTitle(this.identity) ?? this.substitutionService.DefaultSubstitutionText}"
                    : titleText;
            }

            this.PlainText = RemoveHtmlTags(this.HtmlText);
        }

        private static string RemoveHtmlTags(string rawText) => HtmlRemovalRegex.Replace(rawText, string.Empty);
    }
}
