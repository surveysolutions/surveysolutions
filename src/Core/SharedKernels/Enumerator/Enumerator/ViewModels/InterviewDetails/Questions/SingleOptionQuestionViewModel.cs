using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
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
    public class SingleOptionQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel, 
        IDisposable,
        ICompositeQuestionWithChildren,
        IViewModelEventHandler<AnswersRemoved>
    {
        private readonly Guid userId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;
        private readonly QuestionInstructionViewModel instructionViewModel;
        private readonly IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher;
        private readonly ThrottlingViewModel throttlingModel;

        public SingleOptionQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IViewModelEventRegistry eventRegistry,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            FilteredOptionsViewModel filteredOptionsViewModel,
            QuestionInstructionViewModel instructionViewModel,
            ThrottlingViewModel throttlingModel)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (questionnaireRepository == null) throw new ArgumentNullException(nameof(questionnaireRepository));

            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));
            this.eventRegistry = eventRegistry;

            this.questionState = questionStateViewModel;
            this.Answering = answering;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.instructionViewModel = instructionViewModel;
            this.mvxMainThreadDispatcher = Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>();
            this.throttlingModel = throttlingModel;
            this.Options = new CovariantObservableCollection<SingleOptionQuestionOptionViewModel>();

            
        }

        private Identity questionIdentity;

        private Guid interviewId;

        private readonly QuestionStateViewModel<SingleOptionQuestionAnswered> questionState;

        public CovariantObservableCollection<SingleOptionQuestionOptionViewModel> Options { get; private set; }

        public QuestionInstructionViewModel InstructionViewModel => this.instructionViewModel;

        public IQuestionStateViewModel QuestionState => this.questionState;

        public AnsweringViewModel Answering { get; private set; }

        public bool HasOptions => true;

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.filteredOptionsViewModel.Init(interviewId, entityIdentity);

            this.throttlingModel.Init(SaveAnswer);
            
            this.questionIdentity = entityIdentity;
            var interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;

            var singleOptionQuestion = interview.GetSingleOptionQuestion(this.questionIdentity);
            this.previousOptionToReset = singleOptionQuestion.IsAnswered()
                ? singleOptionQuestion.GetAnswer().SelectedValue
                : (int?)null;
            this.UpdateQuestionOptions();

            this.filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;
            this.eventRegistry.Subscribe(this, interviewId);
        }

        private void UpdateQuestionOptions()
        {
            var interview = this.interviewRepository.Get(interviewId.FormatGuid());
            var singleOptionQuestion = interview.GetSingleOptionQuestion(this.questionIdentity);

            List<SingleOptionQuestionOptionViewModel> singleOptionQuestionOptionViewModels = this.filteredOptionsViewModel.GetOptions()
                .Select(model => this.ToViewModel(model, isSelected: singleOptionQuestion.IsAnswered() && model.Value == singleOptionQuestion.GetAnswer().SelectedValue))
                .ToList();
            
            var options = this.Options;
            foreach (var option in options)
            {
                option.BeforeSelected -= this.OptionSelected;
                option.AnswerRemoved -= this.RemoveAnswer;
                option.DisposeIfDisposable();
            }

            this.Options.Clear();
            singleOptionQuestionOptionViewModels.ForEach(x => this.Options.Add(x));
        }

        private async Task FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs eventArgs)
        {
            await this.mvxMainThreadDispatcher.ExecuteOnMainThreadAsync(()=>
            {
                this.UpdateQuestionOptions();
                this.RaisePropertyChanged(() => Options);
            });
        }

        private int? previousOptionToReset = null;
        private int? selectedOptionToSave = null;

        private async Task SaveAnswer()
        {
            if (this.selectedOptionToSave == this.previousOptionToReset)
                return;

            var selectedOption = this.GetOptionByValue(this.selectedOptionToSave);
            var previousOption = this.GetOptionByValue(this.previousOptionToReset);

            if (selectedOption == null)
                return;

            var command = new AnswerSingleOptionQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                selectedOption.Value);

            try
            {
                if (previousOption != null)
                {
                    previousOption.Selected = false;
                }

                await this.Answering.SendQuestionCommandAsync(command).ConfigureAwait(false);

                this.previousOptionToReset = this.selectedOptionToSave;

                await this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                selectedOption.Selected = false;

                if (previousOption != null)
                {
                    previousOption.Selected = true;
                }

                await this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private SingleOptionQuestionOptionViewModel GetOptionByValue(int? value)
        {
            return value.HasValue 
                ? this.Options.FirstOrDefault(x => x.Value == value.Value) 
                : null;
        }

        private async Task OptionSelected(object sender, EventArgs eventArgs)
        {
            var selectedOption = (SingleOptionQuestionOptionViewModel)sender;
            selectedOptionToSave = selectedOption.Value;

            this.Options.Where(x=> x.Selected && x.Value != selectedOptionToSave).ForEach(x => x.Selected = false);

            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        private SingleOptionQuestionOptionViewModel ToViewModel(CategoricalOption model, bool isSelected)
        {
            var optionViewModel = new SingleOptionQuestionOptionViewModel
            {
                Value = model.Value,
                Title = model.Title,
                Selected = isSelected,
                QuestionState = this.questionState,
            };
            optionViewModel.BeforeSelected += this.OptionSelected;
            optionViewModel.AnswerRemoved += this.RemoveAnswer;

            return optionViewModel;
        }

        private async Task RemoveAnswer(object sender, EventArgs e)
        {
            try
            {
                this.throttlingModel.CancelPendingAction();
                await this.Answering.SendQuestionCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.userId,
                        this.questionIdentity));
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

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    foreach (var option in this.Options.Where(option => option.Selected).ToList())
                    {
                        option.Selected = false;
                    }

                    this.previousOptionToReset = null;
                }
            }
        }

        private bool isDisposed = false;

        public void Dispose()
        {
            if (isDisposed)
                return;
            
            isDisposed = true;
            
            this.throttlingModel.Dispose();
            
            this.eventRegistry.Unsubscribe(this);
            this.questionState.Dispose();
            this.InstructionViewModel.Dispose();

            this.filteredOptionsViewModel.OptionsChanged -= FilteredOptionsViewModelOnOptionsChanged;
            this.filteredOptionsViewModel.Dispose();

            var options = this.Options;
            foreach (var option in options)
            {
                option.BeforeSelected -= this.OptionSelected;
                option.AnswerRemoved -= this.RemoveAnswer;
                option.DisposeIfDisposable();
            }
            //options.Clear();
        }

        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                result.Add(new OptionBorderViewModel(this.questionState, true));
                result.AddCollection(this.Options);
                result.Add(new OptionBorderViewModel(this.questionState, false));
                return result;
            }
        }
    }
}
