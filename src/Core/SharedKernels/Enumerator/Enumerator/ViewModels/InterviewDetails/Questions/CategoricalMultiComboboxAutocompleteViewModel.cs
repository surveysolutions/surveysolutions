using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiComboboxAutocompleteViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ICompositeQuestion,
        IDisposable
    {
        public virtual event EventHandler<int> OnAddOption;
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;

        public CategoricalMultiComboboxAutocompleteViewModel(IQuestionStateViewModel questionState,
            FilteredOptionsViewModel filteredOptionsViewModel)
        {
            this.QuestionState = questionState;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.Identity = entityIdentity;
        }

        private int[] excludedOptions;
        public Identity Identity { get; private set; }

        private string filterText;
        public string FilterText
        {
            get => this.filterText;
            set
            {
                this.filterText = value;
                this.RaisePropertyChanged();
            }
        }

        private List<string> autoCompleteSuggestions = new List<string>();
        public List<string> AutoCompleteSuggestions
        {
            get => this.autoCompleteSuggestions;
            set => this.RaiseAndSetIfChanged(ref this.autoCompleteSuggestions, value);
        }

        public IMvxCommand<string> FilterCommand => new MvxCommand<string>(this.UpdateFilter);
        public IMvxCommand RemoveAnswerCommand => new MvxCommand(() => { this.UpdateFilter(null); });
        public IMvxCommand<string> SaveAnswerBySelectedOptionCommand => new MvxCommand<string>(this.SaveAnswerBySelectedOption);
        public IMvxCommand ShowErrorIfNoAnswerCommand => new MvxCommand(() => { });

        private void SaveAnswerBySelectedOption(string optionText)
        {
            optionText = this.RemoveHighlighting(optionText);

            var selectedOption = this.filteredOptionsViewModel.GetOptions(optionText)?.FirstOrDefault()?.Value;
            this.OnAddOption.Invoke(this, selectedOption.Value);

            this.UpdateFilter(null);
        }

        private void UpdateFilter(string filter)
        {
            this.FilterText = filter;
            this.AutoCompleteSuggestions = this.GetHighlightedSuggestions(filter).ToList();
        }

        private string RemoveHighlighting(string optionText) => optionText.Replace("</b>", "").Replace("<b>", "");

        private string GetHighlightedText(string text, string filter)
        {
            var startIndexOfSearchedText = string.IsNullOrEmpty(filter)
                ? -1
                : text.IndexOf(filter, StringComparison.OrdinalIgnoreCase);

            return startIndexOfSearchedText >= 0 ? text.Insert(startIndexOfSearchedText + filter.Length, "</b>")
                .Insert(startIndexOfSearchedText, "<b>") : text;
        }

        private IEnumerable<string> GetHighlightedSuggestions(string filter)
        {
            foreach (var model in this.filteredOptionsViewModel.GetOptions(filter)
                .Where(x => !this.excludedOptions.Contains(x.Value)))
            {
                if (model.Title.IsNullOrEmpty())
                    continue;

                yield return this.GetHighlightedText(model.Title, filter);
            }
        }

        public void ExcludeOptions(int[] excludedOptions) => this.excludedOptions = excludedOptions ?? Array.Empty<int>();

        public void Dispose() { }

        public QuestionInstructionViewModel InstructionViewModel => null;
        public IQuestionStateViewModel QuestionState { get; protected set; }
        public AnsweringViewModel Answering => null;
    }
}
