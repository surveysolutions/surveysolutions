using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class TextQuestionViewModel : InterviewQuestionViewModelBase,
        IInterviewEntityViewModel,
        IViewModelEventHandler<TextQuestionAnswered>,
        IViewModelEventHandler<AnswersRemoved>,
        ICompositeQuestion,
        IDisposable
    {
        private readonly IViewModelEventRegistry liteEventRegistry;
        private readonly IPrincipal principal;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; set; }
        public AnsweringViewModel Answering { get; private set; }

        public TextQuestionViewModel(
            IViewModelEventRegistry liteEventRegistry,
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<TextQuestionAnswered> questionStateViewModel,
            QuestionInstructionViewModel instructionViewModel,
            AnsweringViewModel answering)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.questionState = questionStateViewModel;
        }

        public override void InitFast()
        {
            this.questionState.Init(interviewId, questionIdentity, NavigationState);
            this.InstructionViewModel.Init(interviewId, questionIdentity, NavigationState);
        }
        
        public override void InitData()
        {
            this.InitQuestionSettings();
            this.UpdateSelfFromModel();

            this.liteEventRegistry.Subscribe(this, interviewId);
        }


        public void Dispose()
        {
            this.liteEventRegistry.Unsubscribe(this); 
            this.QuestionState.Dispose();
            this.InstructionViewModel.Dispose();
        }

        private string answer;
        public string Answer
        {
            get { return this.answer; }
            set
            {
                this.answer = value;
                this.RaisePropertyChanged();
            }
        }

        private string mask;
        public string Mask
        {
            get { return this.mask; }
            private set { this.mask = value; this.RaisePropertyChanged(); this.RaisePropertyChanged(() => this.Hint); }
        }

        public string Hint
        {
            get
            {
                if (this.Mask.IsNullOrEmpty()) 
                    return UIResources.TextQuestion_Hint;

                string maskHint = this.Mask.Replace('*', '_').Replace('#', '_').Replace('~', '_');
                return UIResources.TextQuestion_MaskHint.FormatString(maskHint);
            }
        }

        public bool IsMaskedQuestionAnswered { get; set; }
        
        public ICommand ValueChangeCommand => new MvxAsyncCommand<string>(async s => await this.SaveAnswer(s), s => this.principal.IsAuthenticated);

        private IMvxCommand answerRemoveCommand;
        private readonly QuestionStateViewModel<TextQuestionAnswered> questionState;

        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return this.answerRemoveCommand ??= new MvxCommand(async () => await this.RemoveAnswer());
            }
        }

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
        }

        private async Task SaveAnswer(string text)
        {
            if (principal?.CurrentUserIdentity == null)
                throw new InvalidOperationException($"Current principal is not set");

            if (!this.Mask.IsNullOrEmpty() && !this.IsMaskedQuestionAnswered)
            {
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Text_MaskError);
                return;
            }

            if(string.IsNullOrWhiteSpace(text))
            {
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Text_Empty);
                return;
            }

            var command = new AnswerTextQuestionCommand(
                interviewId: Guid.Parse(this.interviewId),
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answer: text);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private void InitQuestionSettings()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.mask = questionnaire.GetTextQuestionMask(this.questionIdentity.Id);
        }

        private void UpdateSelfFromModel()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            
            this.Answer = interview.GetTextQuestion(this.questionIdentity).GetAnswer()?.Value;
        }

        public void Handle(TextQuestionAnswered @event)
        {
            if (@event.QuestionId == this.questionIdentity.Id &&
                @event.RosterVector.SequenceEqual(this.questionIdentity.RosterVector))
            {
                this.UpdateSelfFromModel();
            }
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
