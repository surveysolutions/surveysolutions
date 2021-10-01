using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
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
        public virtual event Func<object, int, Task> OnItemSelected;
        public virtual event Func<object, EventArgs, Task> OnAnswerRemoved;
        public virtual event Func<object, EventArgs, Task> OnShowErrorIfNoAnswer;
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;
        private readonly bool displaySelectedValue;
        private readonly ThrottlingViewModel throttlingModel;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;

        public CategoricalComboboxAutocompleteViewModel(IQuestionStateViewModel questionState,
            FilteredOptionsViewModel filteredOptionsViewModel,
            bool displaySelectedValue,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher)
        {
            this.QuestionState = questionState;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.displaySelectedValue = displaySelectedValue;
            this.throttlingModel = Mvx.IoCProvider.Create<ThrottlingViewModel>();
            this.throttlingModel.Init(UpdateFilterThrottled);
            this.mainThreadDispatcher = mainThreadDispatcher;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.Identity = entityIdentity;
        }

        public void InitFilter(string initialFilter = null)
        {
            this.AutoCompleteSuggestions = this.GetSuggestions(initialFilter).ToList();
            this.FilterText = initialFilter;
            this.RaisePropertyChanged(nameof(FilterText));
        }

        private int[] excludedOptions = Array.Empty<int>();
        public Identity Identity { get; private set; }

        public string FilterText { get; set; }

        private List<OptionWithSearchTerm> autoCompleteSuggestions = new List<OptionWithSearchTerm>();
        public List<OptionWithSearchTerm> AutoCompleteSuggestions
        {
            get => this.autoCompleteSuggestions;
            set => this.SetProperty(ref this.autoCompleteSuggestions, value);
        }
        
        public IMvxCommand RemoveAnswerCommand => new MvxAsyncCommand(async () =>
        {
            this.ResetFilterAndOptions();

            if (this.OnAnswerRemoved == null)
                return;

            await InvokeAllHandlers<EventArgs>(this.OnAnswerRemoved, EventArgs.Empty);
        });

        public IMvxAsyncCommand<string> FilterCommand => new MvxAsyncCommand<string>(x => this.UpdateFilter(x));
        public IMvxCommand<OptionWithSearchTerm> SaveAnswerBySelectedOptionCommand => new MvxCommand<OptionWithSearchTerm>(this.SaveAnswerBySelectedOption);
        public IMvxAsyncCommand ShowErrorIfNoAnswerCommand => new MvxAsyncCommand(this.ShowErrorIfNoAnswer);

        private async Task ShowErrorIfNoAnswer()
        {
            await InvokeAllHandlers(this.OnShowErrorIfNoAnswer, EventArgs.Empty).ConfigureAwait(false);

            if (string.IsNullOrEmpty(this.FilterText)) return;

            var selectedOption = this.filteredOptionsViewModel.GetOptionByTextValue(this.FilterText);

            var isValidOption =
                selectedOption?.Title.Equals(this.FilterText, StringComparison.CurrentCultureIgnoreCase) == true
                && !this.excludedOptions.Contains(selectedOption.Value);

            if (isValidOption)
            {
                await InvokeAllHandlers<int>(this.OnItemSelected, selectedOption.Value).ConfigureAwait(false);
                if (displaySelectedValue)
                {
                    await this.UpdateFilter(selectedOption.Title).ConfigureAwait(false);
                }
                else
                {
                    this.ResetFilterAndOptions();    
                }
            }
            else
            {
                var errorMessage = UIResources.Interview_Question_Filter_MatchError.FormatString(this.FilterText);
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(errorMessage).ConfigureAwait(false);
            }
        }

        private void SaveAnswerBySelectedOption(OptionWithSearchTerm option)
        {
            // When options is selected, FocusOut will be always fired after. 
            // We change filter text and we safe answer on focus out event
            this.FilterText = option.Title;
            this.RaisePropertyChanged(nameof(this.FilterText));
        }

        private async Task InvokeAllHandlers<T>(Func<object, T, Task> handler, T value)
        {
            if (handler == null) return;

            var invocationList = handler.GetInvocationList().Cast<Func<object, T, Task>>();
            var enumerable = invocationList.Select(x => Task.Run(() => x(this, value)));
            await Task.WhenAll(enumerable);
        }

        private string filterToUpdate = null;
        private async Task UpdateFilterThrottled()
        {
            var suggestions = this.GetSuggestions(filterToUpdate).ToList();

            await mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                this.AutoCompleteSuggestions = suggestions;
            });
        }

        public async Task UpdateFilter(string filter, bool forced = false)
        {
            var oldFilterText = this.FilterText;
            this.FilterText = filter;

            if (this.FilterText != oldFilterText)
            {
                await this.RaisePropertyChanged(nameof(FilterText));
            }

            if (this.filterToUpdate == filter && !forced)
            {
                return;
            }

            this.filterToUpdate = filter;
            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        public void ResetFilterAndOptions()
        {
            this.FilterText = null;
            
            var suggestions = this.GetSuggestions(null).ToList();
            this.QuestionState.Validity.ExecutedWithoutExceptions();

            this.InvokeOnMainThread(() =>
            {
                this.AutoCompleteSuggestions = suggestions;
                this.RaisePropertyChanged(nameof(this.FilterText));
            });
        }

        private IEnumerable<OptionWithSearchTerm> GetSuggestions(string filter)
        {
            List<CategoricalOption> filteredOptions = this.filteredOptionsViewModel.GetOptions(filter, this.excludedOptions, 20);

            foreach (var model in filteredOptions.Count == 1 && displaySelectedValue
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

        public void Dispose()
        {
            this.throttlingModel.Dispose();
        }

        public QuestionInstructionViewModel InstructionViewModel => null;
        public IQuestionStateViewModel QuestionState { get; protected set; }
        public AnsweringViewModel Answering => null;
    }
}
