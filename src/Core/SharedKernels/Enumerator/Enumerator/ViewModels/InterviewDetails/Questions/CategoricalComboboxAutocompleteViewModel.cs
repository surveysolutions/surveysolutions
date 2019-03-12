using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalComboboxAutocompleteViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ICompositeQuestion,
        IDisposable
    {
        public virtual event EventHandler<int> OnItemSelected;
        public virtual event EventHandler OnAnswerRemoved;
        public virtual event EventHandler OnShowErrorIfNoAnswer;
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;
        private readonly bool displaySelectedValue;

        public CategoricalComboboxAutocompleteViewModel(IQuestionStateViewModel questionState,
            FilteredOptionsViewModel filteredOptionsViewModel,
            bool displaySelectedValue)
        {
            this.QuestionState = questionState;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.displaySelectedValue = displaySelectedValue;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.Identity = entityIdentity;
            this.UpdateFilter(null);
        }

        private int[] excludedOptions = Array.Empty<int>();
        public Identity Identity { get; private set; }

        public string FilterText { get; set; }

        private List<OptionWithSearchTerm> autoCompleteSuggestions = new List<OptionWithSearchTerm>();
        public List<OptionWithSearchTerm> AutoCompleteSuggestions
        {
            get => this.autoCompleteSuggestions;
            set => this.RaiseAndSetIfChanged(ref this.autoCompleteSuggestions, value);
        }

        public IMvxCommand<string> FilterCommand => new MvxCommand<string>(this.UpdateFilter);
        public IMvxCommand RemoveAnswerCommand => new MvxCommand(() =>
        {
            this.UpdateFilter(null);
            this.OnAnswerRemoved?.Invoke(this, null);
        });

        public IMvxCommand<OptionWithSearchTerm> SaveAnswerBySelectedOptionCommand => new MvxCommand<OptionWithSearchTerm>(this.SaveAnswerBySelectedOption);
        public IMvxCommand ShowErrorIfNoAnswerCommand => new MvxCommand(this.ShowErrorIfNoAnswer);

        private void ShowErrorIfNoAnswer()
        {
            this.OnShowErrorIfNoAnswer?.Invoke(this, EventArgs.Empty);

            if (string.IsNullOrEmpty(this.FilterText)) return;

            var selectedOption = this.filteredOptionsViewModel.GetOptions(this.FilterText).FirstOrDefault(x => !this.excludedOptions.Contains(x.Value));

            if (selectedOption?.Title.Equals(this.FilterText, StringComparison.CurrentCultureIgnoreCase) == true)
                this.SaveAnswerBySelectedOption(ToOptionWithSearchTerm(string.Empty, selectedOption));
            else
            {
                var errorMessage = UIResources.Interview_Question_Filter_MatchError.FormatString(this.FilterText);
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(errorMessage);
            }
        }


        private void SaveAnswerBySelectedOption(OptionWithSearchTerm option)
        {
            this.OnItemSelected?.Invoke(this, option.Value);
            this.UpdateFilter(displaySelectedValue ? option.Title : null);
        }

        public void UpdateFilter(string filter) => this.InvokeOnMainThread(() =>
        {
            this.AutoCompleteSuggestions = this.GetSuggestions(filter).ToList();
            this.FilterText = filter;
            this.RaisePropertyChanged(() => this.FilterText);
        });

        private IEnumerable<OptionWithSearchTerm> GetSuggestions(string filter)
        {
            var filteredOptions = this.filteredOptionsViewModel.GetOptions(filter).ToArray();

            foreach (var model in filteredOptions.Length == 1 && displaySelectedValue
                ? filteredOptions
                : filteredOptions.Where(x => !this.excludedOptions.Contains(x.Value)))
            {
                if (model.Title.IsNullOrEmpty())
                    continue;

                yield return ToOptionWithSearchTerm(filter, model);
            }
        }

        private static OptionWithSearchTerm ToOptionWithSearchTerm(string filter, CategoricalOption model) => new OptionWithSearchTerm
        {
            Value = model.Value,
            Title = model.Title,
            SearchTerm = filter
        };

        public void ExcludeOptions(int[] optionsToExclude) => this.excludedOptions = optionsToExclude ?? Array.Empty<int>();

        public void Dispose() { }

        public QuestionInstructionViewModel InstructionViewModel => null;
        public IQuestionStateViewModel QuestionState { get; protected set; }
        public AnsweringViewModel Answering => null;
    }
}
