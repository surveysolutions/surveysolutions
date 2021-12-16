using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class SingleOptionLinkedToListQuestionViewModel : BaseComboboxQuestionViewModel,
        IAsyncViewModelEventHandler<AnswersRemoved>,
        IViewModelEventHandler<TextListQuestionAnswered>,
        IAsyncViewModelEventHandler<LinkedToListOptionsChanged>,
        IAsyncViewModelEventHandler<QuestionsEnabled>,
        IAsyncViewModelEventHandler<QuestionsDisabled>
    {
        private readonly Guid userId;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;
        private readonly ThrottlingViewModel throttlingModel;

        public SingleOptionLinkedToListQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireStorage,
            IStatefulInterviewRepository interviewRepository,
            IViewModelEventRegistry eventRegistry,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            FilteredOptionsViewModel filteredOptionsViewModel,
            QuestionInstructionViewModel instructionViewModel,
            AnsweringViewModel answering, 
            ThrottlingViewModel throttlingModel):  base(principal: principal, questionStateViewModel: questionStateViewModel, answering: answering,
            instructionViewModel: instructionViewModel, interviewRepository: interviewRepository,
            eventRegistry: eventRegistry, filteredOptionsViewModel, mainThreadDispatcher)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            this.userId = principal.CurrentUserIdentity.UserId;
            this.mainThreadDispatcher = mainThreadDispatcher ?? Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>();

            this.throttlingModel = throttlingModel;
            this.questionnaireRepository = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.throttlingModel.Init(SaveAnswer);
        }

        private Guid linkedToQuestionId;
        private CovariantObservableCollection<SingleOptionQuestionOptionViewModel> options;

        public CovariantObservableCollection<SingleOptionQuestionOptionViewModel> Options
        {
            get => this.options;
            private set
            {
                this.options = value;
                this.RaisePropertyChanged(nameof(HasOptions));
            }
        }

        public bool HasOptions => this.Options.Any() || RenderAsCombobox;

        public override void Init(string interviewId, Identity questionIdentity, NavigationState navigationState)
        {
            base.Init(interviewId, questionIdentity, navigationState);
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire =
                this.questionnaireRepository.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);

            this.interviewId = interviewId;

            this.linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(this.Identity.Id);
            
            var linkedToListQuestion = interview.GetSingleOptionLinkedToListQuestion(this.Identity);
            this.previousOptionToReset = linkedToListQuestion.IsAnswered()
                ? linkedToListQuestion.GetAnswer().SelectedValue
                : (int?) null;

            RenderAsCombobox = questionnaire.IsQuestionFilteredCombobox(questionIdentity.Id);

            this.Options = new CovariantObservableCollection<SingleOptionQuestionOptionViewModel>();
            this.Options.CollectionChanged += OptionsOnCollectionChanged;
            
            this.RefreshOptionsFromModelAsync().WaitAndUnwrapException();
        }

        private void OptionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.optionsTopBorderViewModel != null)
            {
                this.optionsTopBorderViewModel.HasOptions = HasOptions;
            }
            if (this.optionsBottomBorderViewModel != null)
            {
                this.optionsBottomBorderViewModel.HasOptions = this.HasOptions;
            }
        }

        protected override int? GetCurrentAnswer()
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            return interview.GetSingleOptionLinkedToListQuestion(this.Identity).GetAnswer()?.SelectedValue;
        }

        public bool RenderAsCombobox { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            
            this.Options.CollectionChanged -= OptionsOnCollectionChanged;
            foreach (var option in Options)
            {
                option.BeforeSelected -= this.OptionSelected;
                option.AnswerRemoved -= this.RemoveAnswer;
                option.DisposeIfDisposable();
            }
            this.throttlingModel.Dispose();
        }
        
        private async Task OptionSelected(object sender, EventArgs eventArgs) => await this.OptionSelectedAsync(sender);

        private async Task RemoveAnswer(object sender, EventArgs e)
        {
            try
            {
                await this.Answering.SendQuestionCommandAsync(
                    new RemoveAnswerCommand(Guid.Parse(this.interviewId),
                        this.userId,
                        this.Identity));
                await this.QuestionState.Validity.ExecutedWithoutExceptions();

                foreach (var option in this.Options.Where(option => option.Selected).ToList())
                {
                    option.Selected = false;
                }

                this.previousOptionToReset = null;
            }
            catch (InterviewException exception)
            {
                await this.QuestionState.Validity.ProcessException(exception);
            }
        }

        private int? previousOptionToReset = null;
        private int? selectedOptionToSave = null;

        private async Task SaveAnswer()
        {
            if (this.selectedOptionToSave == this.previousOptionToReset)
                return;

            var selectedOption = this.GetOptionByValue(this.selectedOptionToSave);
            var previousOption = this.GetOptionByValue(this.previousOptionToReset);

            var selectedOptionValue = selectedOption?.Value ?? this.selectedOptionToSave;
            if (selectedOptionValue != null)
            {
                var command = new AnswerSingleOptionQuestionCommand(
                    Guid.Parse(this.interviewId),
                    this.userId,
                    this.Identity.Id,
                    this.Identity.RosterVector,
                    selectedOptionValue.Value);

                try
                {
                    if (previousOption != null)
                    {
                        previousOption.Selected = false;
                    }

                    await this.Answering.SendQuestionCommandAsync(command);

                    this.previousOptionToReset = this.selectedOptionToSave;

                    await this.QuestionState.Validity.ExecutedWithoutExceptions();
                }
                catch (InterviewException ex)
                {
                    if (selectedOption != null) selectedOption.Selected = false;

                    if (previousOption != null)
                    {
                        previousOption.Selected = true;
                    }

                    await this.QuestionState.Validity.ProcessException(ex);
                }
            }
        }

        internal async Task OptionSelectedAsync(object sender)
        {
            var selectedOption = (SingleOptionQuestionOptionViewModel) sender;
            this.selectedOptionToSave = selectedOption.Value;
            
            this.Options.Where(x=> x.Selected && x.Value!=selectedOptionToSave).ForEach(x => x.Selected = false);

            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        private SingleOptionQuestionOptionViewModel GetOptionByValue(int? value)
        {
            return value.HasValue 
                ? this.Options.FirstOrDefault(x => x.Value == value.Value) 
                : null;
        }

        public async Task HandleAsync(QuestionsEnabled @event)
        {
            if (@event.Questions.All(x => x.Id != this.linkedToQuestionId)) return;

            await this.RefreshOptionsFromModelAsync();
        }

        public async Task HandleAsync(QuestionsDisabled @event)
        {
            if (@event.Questions.All(x => x.Id != this.linkedToQuestionId))
                return;

            await this.ClearOptionsAsync();
        }

        public async Task HandleAsync(AnswersRemoved @event)
        {
            if (@event.Questions.Contains(this.Identity))
            {
                var optionToDeselect = this.Options.FirstOrDefault(option => option.Selected);
                if (optionToDeselect != null)
                {
                    optionToDeselect.Selected = false;
                }

                this.previousOptionToReset = null;
            }

            if (@event.Questions.Any(question => question.Id == this.linkedToQuestionId))
                await this.ClearOptionsAsync();
        }

        public void Handle(TextListQuestionAnswered @event)
        {
            if (@event.QuestionId != this.linkedToQuestionId)
                return;

            foreach (var answer in @event.Answers)
            {
                var option = this.options.FirstOrDefault(o => o.Value == answer.Item1);

                if (option != null && option.Title != answer.Item2)
                    option.Title = answer.Item2;
            }
        }

        public async Task HandleAsync(LinkedToListOptionsChanged @event)
        {
            if (@event.ChangedLinkedQuestions.All(x => x.QuestionId != this.Identity)) return;

            await this.RefreshOptionsFromModelAsync();
        }

        public override IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                this.optionsTopBorderViewModel =
                    new OptionBorderViewModel(this.QuestionState, true)
                    {
                        HasOptions = HasOptions
                    };
                result.Add(this.optionsTopBorderViewModel);
                if (this.RenderAsCombobox)
                {
                    result.AddCollection(comboboxCollection);
                }
                else
                {
                    result.AddCollection(this.Options);
                }
                    
                this.optionsBottomBorderViewModel =
                    new OptionBorderViewModel(this.QuestionState, false)
                    {
                        HasOptions = HasOptions
                    };
                result.Add(this.optionsBottomBorderViewModel);
                return result;
            }
        }

        private async Task RefreshOptionsFromModelAsync()
        {
            this.comboboxCollection.Remove(this.comboboxViewModel);

            if (RenderAsCombobox)
            {
                this.comboboxCollection.Add(this.comboboxViewModel);
            }
            else
            {
                var textListAnswerRows = this.GetTextListAnswerRows().ToList();

                await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
                {
                    this.RemoveOptions(textListAnswerRows);
                    this.InsertOrUpdateOptions(textListAnswerRows);
                });

                await this.RaisePropertyChanged(() => this.HasOptions);
            }
        }

        private void InsertOrUpdateOptions(List<TextListAnswerRow> textListAnswerRows)
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var linkedQuestionAnswer = interview.GetSingleOptionLinkedToListQuestion(this.Identity).GetAnswer()
                ?.SelectedValue;

            foreach (var textListAnswerRow in textListAnswerRows)
            {
                var viewModelOption = this.options.FirstOrDefault(o => o.Value == textListAnswerRow.Value);
                if (viewModelOption == null)
                {
                    viewModelOption = this.CreateOptionViewModel(textListAnswerRow);
                    this.options.Insert(textListAnswerRows.IndexOf(textListAnswerRow), viewModelOption);
                }

                viewModelOption.Selected = viewModelOption.Value == linkedQuestionAnswer;
            }
        }

        private void RemoveOptions(List<TextListAnswerRow> textListAnswerRows)
        {
            for (int optionIndex = this.options.Count - 1; optionIndex >= 0; optionIndex--)
            {
                var optionToRemove = this.options[optionIndex];
                if (textListAnswerRows?.Any(x => x.Value == optionToRemove.Value && x.Text == optionToRemove.Title) ??
                    false) continue;

                optionToRemove.BeforeSelected -= this.OptionSelected;
                optionToRemove.AnswerRemoved -= this.RemoveAnswer;

                this.options.Remove(optionToRemove);
            }
        }

        private async Task ClearOptionsAsync() =>
            await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                this.options.ForEach(option =>
                {
                    option.BeforeSelected -= this.OptionSelected;
                    option.AnswerRemoved -= this.RemoveAnswer;
                });
                this.options.Clear();
                this.RaisePropertyChanged(() => this.HasOptions);
            });

        private IEnumerable<TextListAnswerRow> GetTextListAnswerRows()
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var listQuestion = interview.FindQuestionInQuestionBranch(this.linkedToQuestionId, this.Identity);
            if (listQuestion == null || listQuestion.IsDisabled()) yield break;

            var listOptions = listQuestion.GetAsInterviewTreeTextListQuestion().GetAnswer()?.Rows;
            var filteredOptions = interview.GetSingleOptionLinkedToListQuestion(this.Identity)?.Options;
            
            if (listOptions == null || filteredOptions == null) yield break;

            foreach (var textListAnswerRow in listOptions)
            {
                if (filteredOptions.Contains(textListAnswerRow.Value))
                    yield return textListAnswerRow;
            }
        }

        private SingleOptionQuestionOptionViewModel CreateOptionViewModel(TextListAnswerRow optionValue)
        {
            var option = new SingleOptionQuestionOptionViewModel
            {
                Enablement = this.QuestionState.Enablement,
                Title = optionValue.Text,
                Value = optionValue.Value,
                QuestionState = this.QuestionState
            };

            option.BeforeSelected += this.OptionSelected;
            option.AnswerRemoved += this.RemoveAnswer;

            return option;
        }
    }
}
