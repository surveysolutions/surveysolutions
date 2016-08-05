using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class IntegerQuestionViewModel : 
        MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel, 
        ILiteEventHandler<AnswerRemoved>,
        IDisposable
    {
        internal const int RosterUpperBoundDefaultValue = Constants.MaxRosterRowCount;

        private readonly IPrincipal principal; 
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInteractionService userInteractionService;
        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<NumericIntegerQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        private bool isRosterSizeQuestion;
        private int answerMaxValue;

        private decimal? previousAnswer;
        private decimal? answer;
        public decimal? Answer
        {
            get { return this.answer; }
            set
            {
                if (this.answer != value)
                {
                    this.answer = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        
        public bool UseFormatting { get; set; }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return this.valueChangeCommand ?? (this.valueChangeCommand = new MvxCommand(async () => await this.SendAnswerIntegerQuestionCommandAsync())); }
        }  
        
        private IMvxCommand answerRemoveCommand;

        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return this.answerRemoveCommand ??
                       (this.answerRemoveCommand = new MvxCommand(async () => await this.RemoveAnswer()));
            }
        }

        private async Task RemoveAnswer()
        {
            if (!QuestionState.IsAnswered)
            {
                this.Answer = null;
                this.QuestionState.Validity.ExecutedWithoutExceptions();
                return;
            }
            try
            {
                if (isRosterSizeQuestion)
                {
                    var amountOfRostersToRemove = this.previousAnswer;
                    var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage,
                        amountOfRostersToRemove);
                    if (!(await this.userInteractionService.ConfirmAsync(message)))
                    {
                        this.Answer = this.previousAnswer;
                        return;
                    }
                }
                var command = new RemoveAnswerCommand(Guid.Parse(this.interviewId),
                    this.principal.CurrentUserIdentity.UserId,
                    this.questionIdentity,
                    DateTime.UtcNow);
                await this.Answering.SendRemoveAnswerCommandAsync(command);

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public IntegerQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<NumericIntegerQuestionAnswered> questionStateViewModel,
            IUserInteractionService userInteractionService,
            AnsweringViewModel answering, ILiteEventRegistry liteEventRegistry)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.userInteractionService = userInteractionService;
            this.Answering = answering;
            this.liteEventRegistry = liteEventRegistry;
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;
            this.liteEventRegistry.Subscribe(this, interviewId);
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetIntegerNumericAnswer(entityIdentity);

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.UseFormatting = questionnaire.ShouldUseFormatting(entityIdentity.Id);

            if (answerModel.IsAnswered)
            {
                this.Answer = answerModel.Answer;
                this.previousAnswer = Monads.Maybe(() => answerModel.Answer);
            }
            this.isRosterSizeQuestion = questionnaire.ShouldQuestionSpecifyRosterSize(entityIdentity.Id);
            this.answerMaxValue = RosterUpperBoundDefaultValue;
        }

        private async Task SendAnswerIntegerQuestionCommandAsync()
        {
            if (this.Answer == null)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Integer_EmptyValueError);
                return;
            }
            
            if (this.Answer > int.MaxValue || this.Answer < int.MinValue)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Integer_ParsingError);
                return;
            }

            if (this.isRosterSizeQuestion)
            {
                if (this.Answer < 0)
                {
                    var message = string.Format(UIResources.Interview_Question_Integer_NegativeRosterSizeAnswer, this.Answer);
                    this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                    return;
                }

                if (this.Answer > this.answerMaxValue)
                {
                    var message = string.Format(UIResources.Interview_Question_Integer_RosterSizeAnswerMoreThanMaxValue, this.Answer, this.answerMaxValue);
                    this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                    return;
                }

                if (this.previousAnswer.HasValue && this.Answer < this.previousAnswer)
                {
                    var amountOfRostersToRemove = this.previousAnswer - this.Answer;
                    var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                    if (!(await this.userInteractionService.ConfirmAsync(message)))
                    {
                        this.Answer = this.previousAnswer;
                        return;
                    }
                }
            }

            var command = new AnswerNumericIntegerQuestionCommand(
                interviewId: Guid.Parse(this.interviewId),
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answer: (int)this.Answer);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();

                this.previousAnswer = this.Answer;
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public void Dispose()
        {
            this.liteEventRegistry.Unsubscribe(this); 
            this.QuestionState.Dispose();
        }

        public void Handle(AnswerRemoved @event)
        {
            if (@event.QuestionId == this.questionIdentity.Id &&
                @event.RosterVector.SequenceEqual(this.questionIdentity.RosterVector))
            {
                this.Answer = null;

                this.previousAnswer = null;
            }
        }
    }
}