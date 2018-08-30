using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
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
        ILiteEventHandler<AnswersRemoved>
    {
        private readonly Guid userId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly FilteredOptionsViewModel filteredOptionsViewModel;
        private readonly QuestionInstructionViewModel instructionViewModel;
        private readonly IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher;

        public SingleOptionQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            FilteredOptionsViewModel filteredOptionsViewModel,
            QuestionInstructionViewModel instructionViewModel,
            IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (questionnaireRepository == null) throw new ArgumentNullException(nameof(questionnaireRepository));
            if (interviewRepository == null) throw new ArgumentNullException(nameof(interviewRepository));

            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;

            this.questionState = questionStateViewModel;
            this.Answering = answering;
            this.filteredOptionsViewModel = filteredOptionsViewModel;
            this.instructionViewModel = instructionViewModel;
            this.mvxMainThreadDispatcher = mvxMainThreadDispatcher;
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

            this.instructionViewModel.Init(interviewId, entityIdentity);
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.filteredOptionsViewModel.Init(interviewId, entityIdentity, 200);

            this.questionIdentity = entityIdentity;
            var interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;

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

            this.Options.ForEach(x => x.DisposeIfDisposable());
            this.Options.Clear();
            singleOptionQuestionOptionViewModels.ForEach(x => this.Options.Add(x));
        }

        private async void FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs eventArgs)
        {
            await this.mvxMainThreadDispatcher.ExecuteOnMainThreadAsync(()=>
            {
                this.UpdateQuestionOptions();
                this.RaisePropertyChanged(() => Options);
            });
        }

        private async void OptionSelected(object sender, EventArgs eventArgs)
        {
            var selectedOption = (SingleOptionQuestionOptionViewModel)sender;
            var previousOption = this.Options.SingleOrDefault(option => option.Selected && option != selectedOption);

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

                await this.Answering.SendAnswerQuestionCommandAsync(command).ConfigureAwait(false);

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

        private SingleOptionQuestionOptionViewModel ToViewModel(CategoricalOption model, bool isSelected)
        {
            var optionViewModel = new SingleOptionQuestionOptionViewModel
            {
                Enablement = this.questionState.Enablement,
                Value = model.Value,
                Title = model.Title,
                Selected = isSelected,
                QuestionState = this.questionState,
            };
            optionViewModel.BeforeSelected += this.OptionSelected;
            optionViewModel.AnswerRemoved += this.RemoveAnswer;

            return optionViewModel;
        }

        private async void RemoveAnswer(object sender, EventArgs e)
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.userId,
                        this.questionIdentity));
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
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
                }
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.questionState.Dispose();

            this.filteredOptionsViewModel.OptionsChanged -= FilteredOptionsViewModelOnOptionsChanged;
            this.filteredOptionsViewModel.Dispose();

            foreach (var option in this.Options)
            {
                option.BeforeSelected -= this.OptionSelected;
                option.AnswerRemoved -= this.RemoveAnswer;
            }
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
