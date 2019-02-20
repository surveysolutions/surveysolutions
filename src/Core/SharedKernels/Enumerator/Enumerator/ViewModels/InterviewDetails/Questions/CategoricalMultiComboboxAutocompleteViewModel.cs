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

        private List<OptionWithSearchTerm> autoCompleteSuggestions = new List<OptionWithSearchTerm>();
        public List<OptionWithSearchTerm> AutoCompleteSuggestions
        {
            get => this.autoCompleteSuggestions;
            set => this.RaiseAndSetIfChanged(ref this.autoCompleteSuggestions, value);
        }

        public IMvxCommand<string> FilterCommand => new MvxCommand<string>(this.UpdateFilter);
        public IMvxCommand RemoveAnswerCommand => new MvxCommand(() => { this.UpdateFilter(null); });
        public IMvxCommand<OptionWithSearchTerm> SaveAnswerBySelectedOptionCommand => new MvxCommand<OptionWithSearchTerm>(this.SaveAnswerBySelectedOption);
        public IMvxCommand ShowErrorIfNoAnswerCommand => new MvxCommand(() => { });

        private void SaveAnswerBySelectedOption(OptionWithSearchTerm option)
        {
            this.OnAddOption.Invoke(this, option.Value);

            this.UpdateFilter(null);
        }

        private void UpdateFilter(string filter)
        {
            this.FilterText = filter;
            this.AutoCompleteSuggestions = this.GetSuggestions(filter).ToList();
        }


        private IEnumerable<OptionWithSearchTerm> GetSuggestions(string filter)
        {
            foreach (var model in this.filteredOptionsViewModel.GetOptions(filter)
                .Where(x => !this.excludedOptions.Contains(x.Value)))
            {
                if (model.Title.IsNullOrEmpty())
                    continue;

                yield return new OptionWithSearchTerm
                {
                    Value = model.Value,
                    Title = model.Title,
                    SearchTerm = filter
                };
            }
        }

        public void ExcludeOptions(int[] excludedOptions) => this.excludedOptions = excludedOptions ?? Array.Empty<int>();

        public void Dispose() { }

        public QuestionInstructionViewModel InstructionViewModel => null;
        public IQuestionStateViewModel QuestionState { get; protected set; }
        public AnsweringViewModel Answering => null;
    }
}
