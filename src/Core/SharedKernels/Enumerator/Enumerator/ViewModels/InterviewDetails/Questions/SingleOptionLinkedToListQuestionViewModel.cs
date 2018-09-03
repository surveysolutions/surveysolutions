using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class SingleOptionLinkedToListQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<TextListQuestionAnswered>,
        ILiteEventHandler<LinkedToListOptionsChanged>,
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<QuestionsDisabled>,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        private readonly Guid userId;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;
        protected IStatefulInterview interview;

        public SingleOptionLinkedToListQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireStorage,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            QuestionInstructionViewModel instructionViewModel,
            AnsweringViewModel answering)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));
            this.eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
            this.mainThreadDispatcher = mainThreadDispatcher ?? Mvx.Resolve<IMvxMainThreadAsyncDispatcher>();

            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.questionnaireRepository = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.timer = new Timer(async _ => { await SaveAnswer(); }, null, Timeout.Infinite, Timeout.Infinite);
        }

        private Guid interviewId;
        private Guid linkedToQuestionId;
        private CovariantObservableCollection<SingleOptionQuestionOptionViewModel> options;
        private readonly QuestionStateViewModel<SingleOptionQuestionAnswered> questionState;
        private OptionBorderViewModel optionsTopBorderViewModel;
        private OptionBorderViewModel optionsBottomBorderViewModel;

        public CovariantObservableCollection<SingleOptionQuestionOptionViewModel> Options
        {
            get => this.options;
            private set
            {
                this.options = value;
                this.RaisePropertyChanged(() => this.HasOptions);
            }
        }

        public bool HasOptions => this.Options.Any();

        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; set; }
        public AnsweringViewModel Answering { get; private set; }

        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity questionIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (questionIdentity == null) throw new ArgumentNullException(nameof(questionIdentity));

            this.questionState.Init(interviewId, questionIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, questionIdentity);

            this.interview = this.interviewRepository.Get(interviewId);
            var questionnaire =
                this.questionnaireRepository.GetQuestionnaire(this.interview.QuestionnaireIdentity, interview.Language);

            this.Identity = questionIdentity;
            this.interviewId = interview.Id;

            this.linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(this.Identity.Id);


            var linkedToListQuestion = interview.GetSingleOptionLinkedToListQuestion(this.Identity);
            this.previousOptionToReset = linkedToListQuestion.IsAnswered()
                ? linkedToListQuestion.GetAnswer().SelectedValue
                : (int?) null;

            this.Options = new CovariantObservableCollection<SingleOptionQuestionOptionViewModel>();
            this.Options.CollectionChanged += (sender, args) =>
            {
                if (this.optionsTopBorderViewModel != null)
                {
                    this.optionsTopBorderViewModel.HasOptions = HasOptions;
                }
                if (this.optionsBottomBorderViewModel != null)
                {
                    this.optionsBottomBorderViewModel.HasOptions = this.HasOptions;
                }
            };
            
            this.eventRegistry.Subscribe(this, interviewId);

            this.RefreshOptionsFromModelAsync();
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();

            foreach (var option in Options)
            {
                option.BeforeSelected -= this.OptionSelected;
                option.AnswerRemoved -= this.RemoveAnswer;
            }
        }

        private async void OptionSelected(object sender, EventArgs eventArgs) => await this.OptionSelectedAsync(sender);

        private async void RemoveAnswer(object sender, EventArgs e)
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.userId,
                        this.Identity));
                this.QuestionState.Validity.ExecutedWithoutExceptions();

                foreach (var option in this.Options.Where(option => option.Selected).ToList())
                {
                    option.Selected = false;
                }

                this.previousOptionToReset = null;
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }

        private readonly Timer timer;
        protected internal int ThrottlePeriod { get; set; } = Constants.ThrottlePeriod;
        private int? previousOptionToReset = null;
        private int? selectedOptionToSave = null;
		
        private async Task SaveAnswer()
        {
            if (this.selectedOptionToSave == this.previousOptionToReset)
                return;

            var selectedOption = this.GetOptionByValue(this.selectedOptionToSave);
            var previousOption = this.GetOptionByValue(this.previousOptionToReset);

            var command = new AnswerSingleOptionQuestionCommand(
                this.interviewId,
                this.userId,
                this.Identity.Id,
                this.Identity.RosterVector,
                selectedOption.Value);

            try
            {
                if (previousOption != null)
                {
                    previousOption.Selected = false;
                }

                await this.Answering.SendAnswerQuestionCommandAsync(command);

                this.previousOptionToReset = this.selectedOptionToSave;

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                selectedOption.Selected = false;

                if (previousOption != null)
                {
                    previousOption.Selected = true;
                }

                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        internal async Task OptionSelectedAsync(object sender)
        {
            var selectedOption = (SingleOptionQuestionOptionViewModel) sender;
            this.selectedOptionToSave = selectedOption.Value;
            
            this.Options.Where(x=> x.Selected && x.Value!=selectedOptionToSave).ForEach(x => x.Selected = false);

            if (this.ThrottlePeriod == 0)
            {
                await SaveAnswer();
            }
            else
            {
                timer.Change(ThrottlePeriod, Timeout.Infinite);
            }
        }

        private SingleOptionQuestionOptionViewModel GetOptionByValue(int? value)
        {
            return value.HasValue 
                ? this.Options.FirstOrDefault(x => x.Value == value.Value) 
                : null;
        }

        public async void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.All(x => x.Id != this.linkedToQuestionId)) return;

            await this.RefreshOptionsFromModelAsync();
        }

        public async void Handle(QuestionsDisabled @event)
        {
            if (@event.Questions.All(x => x.Id != this.linkedToQuestionId))
                return;

            await this.ClearOptionsAsync();
        }

        public async void Handle(AnswersRemoved @event)
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

        public async void Handle(LinkedToListOptionsChanged @event)
        {
            if (@event.ChangedLinkedQuestions.All(x => x.QuestionId != this.Identity)) return;

            await this.RefreshOptionsFromModelAsync();
        }

        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                this.optionsTopBorderViewModel =
                    new OptionBorderViewModel(this.questionState, true)
                    {
                        HasOptions = HasOptions
                    };
                result.Add(this.optionsTopBorderViewModel);
                result.AddCollection(this.Options);
                this.optionsBottomBorderViewModel =
                    new OptionBorderViewModel(this.questionState, false)
                    {
                        HasOptions = HasOptions
                    };
                result.Add(this.optionsBottomBorderViewModel);
                return result;
            }
        }

        private async Task RefreshOptionsFromModelAsync()
        {
            var textListAnswerRows = this.GetTextListAnswerRows();

            await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                this.RemoveOptions(textListAnswerRows);
                this.InsertOrUpdateOptions(textListAnswerRows);
            });

            this.RaisePropertyChanged(() => this.HasOptions);
        }

        private void InsertOrUpdateOptions(List<TextListAnswerRow> textListAnswerRows)
        {
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
                this.options.Clear();
                this.RaisePropertyChanged(() => this.HasOptions);
            });

        private List<TextListAnswerRow> GetTextListAnswerRows()
        {
            var listQuestion = interview.FindQuestionInQuestionBranch(this.linkedToQuestionId, this.Identity);

            if ((listQuestion == null) || listQuestion.IsDisabled() ||
                listQuestion.GetAsInterviewTreeTextListQuestion().GetAnswer()?.Rows == null)
                return new List<TextListAnswerRow>();

            return new List<TextListAnswerRow>(listQuestion.GetAsInterviewTreeTextListQuestion().GetAnswer().Rows);
        }

        private SingleOptionQuestionOptionViewModel CreateOptionViewModel(TextListAnswerRow optionValue)
        {
            var option = new SingleOptionQuestionOptionViewModel
            {
                Enablement = this.questionState.Enablement,
                Title = optionValue.Text,
                Value = optionValue.Value,
                QuestionState = this.questionState
            };

            option.BeforeSelected += this.OptionSelected;
            option.AnswerRemoved += this.RemoveAnswer;

            return option;
        }
    }
}
