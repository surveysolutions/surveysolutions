using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.Identity = entityIdentity;
            await this.UpdateFilter(FilterText);
        }

        private int[] excludedOptions = Array.Empty<int>();
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

        public IMvxAsyncCommand<string> FilterCommand => new MvxAsyncCommand<string>(this.UpdateFilter);
        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(async() =>
        {
            await this.UpdateFilter(null);
            this.OnAnswerRemoved?.Invoke(this,null);
        });
        public IMvxAsyncCommand<OptionWithSearchTerm> SaveAnswerBySelectedOptionCommand => new MvxAsyncCommand<OptionWithSearchTerm>(this.SaveAnswerBySelectedOption);
        public IMvxCommand ShowErrorIfNoAnswerCommand => new MvxCommand( ShowErrorIfNoAnswer);

        public IMvxCommand<OptionWithSearchTerm> UpdateText => new MvxCommand<OptionWithSearchTerm>(this.UpdateTextValue);

        private void UpdateTextValue(OptionWithSearchTerm arg)
        {
            if (arg != null)
                FilterText = arg.Title;
        }


        private void ShowErrorIfNoAnswer()
        {
            if (string.IsNullOrEmpty(this.FilterText)) return;

            var selectedOption = this.filteredOptionsViewModel.GetOptions(this.FilterText).FirstOrDefault();

            if (selectedOption != null) return;

            var errorMessage = UIResources.Interview_Question_Filter_MatchError.FormatString(this.FilterText);
            this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(errorMessage);
        }


        private async Task SaveAnswerBySelectedOption(OptionWithSearchTerm option)
        {
            this.OnItemSelected?.Invoke(this, option.Value);
            await this.UpdateFilter(displaySelectedValue ? option.Title : null);
        }

        public async Task UpdateFilter(string filter)
        {
            this.FilterText = filter;
            this.AutoCompleteSuggestions = await Task.Run(() => this.GetSuggestions(filter).ToList());
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

        public void ExcludeOptions(int[] optionsToExclude) => this.excludedOptions = optionsToExclude ?? Array.Empty<int>();

        public void Dispose() { }

        public QuestionInstructionViewModel InstructionViewModel => null;
        public IQuestionStateViewModel QuestionState { get; protected set; }
        public AnsweringViewModel Answering => null;
    }
}
