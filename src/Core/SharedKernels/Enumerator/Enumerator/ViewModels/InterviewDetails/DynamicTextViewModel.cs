using System;
using System.Linq;
using System.Text.RegularExpressions;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class DynamicTextViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<SubstitutionTitlesChanged>,
        ILiteEventHandler<VariablesChanged>,
        IDisposable
    {
        private static readonly Regex HtmlRemovalRegex = new Regex("<.*?>");

        private readonly ILiteEventRegistry eventRegistry;
        private readonly SubstitutionViewModel substitutionViewModel;

        public DynamicTextViewModel(
            ILiteEventRegistry eventRegistry,
            SubstitutionViewModel substitutionViewModel)
        {
            this.eventRegistry = eventRegistry;
            this.substitutionViewModel = substitutionViewModel;
        }

        private Identity identity;

        public void InitAsStatic(string textWithoutSubstitutions)
        {
            if (textWithoutSubstitutions == null) throw new ArgumentNullException(nameof(textWithoutSubstitutions));

            this.HtmlText = textWithoutSubstitutions;
            this.PlainText = textWithoutSubstitutions;
        }

        public void Init(string interviewId, Identity entityIdentity, string textWithSubstitutions)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.identity = entityIdentity;

            this.substitutionViewModel.Init(interviewId, entityIdentity, textWithSubstitutions);

            this.RefreshTexts();

            this.eventRegistry.Subscribe(this, interviewId);
        }

        public void ChangeText(string textWithSubstitutions)
        {
            this.substitutionViewModel.ChangeText(textWithSubstitutions);

            this.RefreshTexts();
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

        public void Handle(VariablesChanged @event)
        {
            // this is needed because update is not published if variable changes

            bool shouldUpdateTexts =
                this.substitutionViewModel.HasVariablesInText(
                    @event.ChangedVariables.Select(variable => variable.Identity));

            if (!shouldUpdateTexts) return;

            this.RefreshTexts();
        }

        public void Handle(SubstitutionTitlesChanged @event)
        {
            bool shouldUpdateTexts =
                @event.Questions.Contains(this.identity) ||
                @event.StaticTexts.Contains(this.identity);

            if (!shouldUpdateTexts) return;

            this.RefreshTexts();
        }

        private void RefreshTexts()
        {
            this.HtmlText = this.substitutionViewModel.ReplaceSubstitutions();
            this.PlainText = RemoveHtmlTags(this.HtmlText);
        }

        private static string RemoveHtmlTags(string rawText) => HtmlRemovalRegex.Replace(rawText, string.Empty);
    }
}