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
        ILiteEventHandler<AnswersRemoved>,
        ICompositeQuestion,
        IDisposable
    {
        private string interviewId;

        public event EventHandler AnswerRemoved;

        private readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry liteEventRegistry;
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
            ILiteEventRegistry liteEventRegistry)
        {
            this.principal = principal;
            this.interviewRepository = interviewRepository;

            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.liteEventRegistry = liteEventRegistry;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity);

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;
            this.liteEventRegistry.Subscribe(this, interviewId);

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetDateTimeQuestion(entityIdentity);
            this.answerFormatString = answerModel.UiFormatString;
            if (answerModel.IsAnswered())
            {
                this.SetToView(answerModel.GetAnswer().Value);
            }
        }

        private Identity questionIdentity;
        public Identity Identity => this.questionIdentity;

        public IMvxAsyncCommand SaveAnswerCommand => new MvxAsyncCommand(this.SendAnswer);

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
                await this.Answering.SendRemoveAnswerCommandAsync(command);

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }

            this.AnswerRemoved?.Invoke(this, EventArgs.Empty);
        }

        private async Task SendAnswer()
        {
            var currentTime = DateTime.Now;
            try
            {
                var command = new AnswerDateTimeQuestionCommand(
                    interviewId: Guid.Parse(this.interviewId),
                    userId: this.principal.CurrentUserIdentity.UserId,
                    questionId: this.questionIdentity.Id,
                    rosterVector: this.questionIdentity.RosterVector,
                    answer: currentTime
                    );

                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.SetToView(currentTime);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
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
            get { return this.answer; }
            set { this.answer = value; this.RaisePropertyChanged(); }
        }

        public void Dispose()
        {
            this.QuestionState.Dispose();
            this.liteEventRegistry.Unsubscribe(this); 
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
