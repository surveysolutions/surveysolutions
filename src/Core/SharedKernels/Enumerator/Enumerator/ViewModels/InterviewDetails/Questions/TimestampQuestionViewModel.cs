using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class TimestampQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IViewModelEventHandler<AnswersRemoved>,
        ICompositeQuestion,
        IDisposable
    {
        private string interviewId;

        public event EventHandler AnswerRemoved;

        private readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelEventRegistry liteEventRegistry;
        private readonly QuestionStateViewModel<DateTimeQuestionAnswered> questionState;
        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; }

        public AnsweringViewModel Answering { get; private set; }

        public TimestampQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<DateTimeQuestionAnswered> questionStateViewModel,
            QuestionInstructionViewModel instructionViewModel,
            AnsweringViewModel answering, 
            IViewModelEventRegistry liteEventRegistry)
        {
            this.principal = principal;
            this.interviewRepository = interviewRepository;

            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.liteEventRegistry = liteEventRegistry;

            this.SaveAnswerCommand = new MvxAsyncCommand(this.SendAnswer);
            ShouldAlwaysRaiseInpcOnUserInterfaceThread(true);
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;
            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetDateTimeQuestion(entityIdentity);
            this.answerFormatString = answerModel.UiFormatString;
            if (answerModel.IsAnswered())
            {
                this.SetToView(answerModel.GetAnswer().Value);
            }

            this.liteEventRegistry.Subscribe(this, interviewId);
        }

        private Identity questionIdentity;
        public Identity Identity => this.questionIdentity;

        public IMvxAsyncCommand SaveAnswerCommand { get; }

        private IMvxAsyncCommand answerRemoveCommand;
        public IMvxAsyncCommand RemoveAnswerCommand
            => this.answerRemoveCommand ?? (this.answerRemoveCommand = new MvxAsyncCommand(this.RemoveAnswer));

        private async Task RemoveAnswer()
        {
            try
            {
                var command = new RemoveAnswerCommand(Guid.Parse(this.interviewId),
                    this.principal.CurrentUserIdentity.UserId,
                    this.questionIdentity);
                await this.Answering.SendQuestionCommandAsync(command);

                await this.QuestionState.Validity.ExecutedWithoutExceptions();

                this.AnswerRemoved?.Invoke(this, EventArgs.Empty);
            }
            catch (InterviewException ex)
            {
                await this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private async Task SendAnswer()
        {
            var newAnswer = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            try
            {
                var command = new AnswerDateTimeQuestionCommand(
                    interviewId: Guid.Parse(this.interviewId),
                    userId: this.principal.CurrentUserIdentity.UserId,
                    questionId: this.questionIdentity.Id,
                    rosterVector: this.questionIdentity.RosterVector,
                    answer: newAnswer
                );

                await this.Answering.SendQuestionCommandAsync(command);
                this.SetToView(newAnswer);
                await this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                await this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private void SetToView(DateTime answerValue)
        {
            this.Answer = answerValue.ToString(answerFormatString);
        }

        private string answer;
        private string answerFormatString;

        public string Answer
        {
            get => this.answer;
            set => this.SetProperty(ref this.answer, value);
        }

        public void Dispose()
        {
            this.liteEventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
            this.InstructionViewModel.Dispose();
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    this.Answer = string.Empty;
                }
            }
        }
    }
}
