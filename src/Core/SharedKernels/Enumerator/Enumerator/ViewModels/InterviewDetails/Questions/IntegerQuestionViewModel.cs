using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class IntegerQuestionViewModel :
        MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        private readonly IPrincipal principal;
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInteractionService userInteractionService;
        private readonly SpecialValuesViewModel specialValues;
        private Identity questionIdentity;
        private string interviewId;

        public IQuestionStateViewModel QuestionState => this.questionState;
        public SpecialValuesViewModel SpecialValues => this.specialValues;

        public QuestionInstructionViewModel InstructionViewModel { get; }

        public AnsweringViewModel Answering { get; private set; }

        private bool isRosterSizeQuestion;
        private int answerMaxValue;

        private decimal? previousAnswer;

        private decimal? answer;
        public decimal? Answer
        {
            get => this.answer;
            set
            {
                if (this.answer == value) return;
                this.answer = value;
                this.RaisePropertyChanged();
            }
        }

        public bool UseFormatting { get; set; }

        private IMvxCommand valueChangeCommand;

        public IMvxCommand ValueChangeCommand
        {
            get
            {
                return this.valueChangeCommand ?? (this.valueChangeCommand =
                           new MvxCommand(async () => await this.SendAnswerIntegerQuestionCommandAsync()));
            }
        }

        private IMvxCommand answerRemoveCommand;
        private readonly QuestionStateViewModel<NumericIntegerQuestionAnswered> questionState;
        

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
            if (!questionState.IsAnswered)
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
                        this.SpecialValues.SetAnswer(this.previousAnswer);
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
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            ILiteEventRegistry liteEventRegistry,
            SpecialValuesViewModel specialValues)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.questionState = questionStateViewModel;
            this.userInteractionService = userInteractionService;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.liteEventRegistry = liteEventRegistry;
            this.specialValues = specialValues;
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionIdentity = entityIdentity ?? throw new ArgumentNullException(nameof(entityIdentity));
            this.interviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));

            this.liteEventRegistry.Subscribe(this, interviewId);
            this.questionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);

            this.InstructionViewModel.Init(interviewId, entityIdentity);

            var questionnaire =
                this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.UseFormatting = questionnaire.ShouldUseFormatting(entityIdentity.Id);

            var answerModel = interview.GetIntegerQuestion(entityIdentity);
            if (answerModel.IsAnswered())
            {
                this.Answer = answerModel.GetAnswer().Value;
                this.previousAnswer = this.Answer;
            }

            this.isRosterSizeQuestion = questionnaire.IsRosterSizeQuestion(entityIdentity.Id);
            var isRosterSizeOfLongRoster = questionnaire.IsQuestionIsRosterSizeForLongRoster(entityIdentity.Id);

            if (isRosterSizeOfLongRoster)
                this.answerMaxValue = Constants.MaxLongRosterRowCount;
            else
                this.answerMaxValue = Constants.MaxRosterRowCount;

            specialValues.Init(interviewId, entityIdentity, this.questionState);
            this.specialValues.SpecialValueChanged += SpecialValueChanged;
            this.specialValues.SpecialValueRemoved += SpecialValueRemoved;
        }

        private async void SpecialValueChanged(object sender, EventArgs eventArgs)
        {
            var selectedSpecialValue = (SingleOptionQuestionOptionViewModel)sender;
            await SaveAnswer(selectedSpecialValue.Value, true);
        }

        private async void SpecialValueRemoved(object sender, EventArgs eventArgs)
        {
            await RemoveAnswer();
        }

        private async Task SendAnswerIntegerQuestionCommandAsync()
        {
            await SaveAnswer(this.Answer, specialValues.IsSpecialValueSelected(this.Answer));
        }

        private async Task SaveAnswer(decimal? answeredOrSelectedValue, bool isSpecialValueSelected)
        {
            if (answeredOrSelectedValue == null)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources
                    .Interview_Question_Integer_EmptyValueError);
                return;
            }

            if (answeredOrSelectedValue > int.MaxValue || answeredOrSelectedValue < int.MinValue)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources
                    .Interview_Question_Integer_ParsingError);
                return;
            }

            if (this.isRosterSizeQuestion)
            {
                if (!isSpecialValueSelected && answeredOrSelectedValue < 0)
                {
                    var message = string.Format(UIResources.Interview_Question_Integer_NegativeRosterSizeAnswer,
                        answeredOrSelectedValue);
                    this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                    return;
                }

                if (answeredOrSelectedValue > this.answerMaxValue)
                {
                    var message = string.Format(UIResources.Interview_Question_Integer_RosterSizeAnswerMoreThanMaxValue,
                        answeredOrSelectedValue, this.answerMaxValue);
                    this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                    return;
                }

                if (this.previousAnswer.HasValue && answeredOrSelectedValue < this.previousAnswer)
                {
                    var amountOfRostersToRemove = this.previousAnswer - answeredOrSelectedValue;
                    var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage,
                        amountOfRostersToRemove);
                    if (!(await this.userInteractionService.ConfirmAsync(message)))
                    {
                        this.Answer = this.previousAnswer;
                        this.specialValues.SetAnswer(this.previousAnswer);
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
                answer: (int) answeredOrSelectedValue);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();

                this.previousAnswer = answeredOrSelectedValue;

                if (isSpecialValueSelected)
                {
                    this.Answer = null;
                }

                this.specialValues.SetAnswer(answeredOrSelectedValue);
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

            this.specialValues.SpecialValueChanged -= SpecialValueChanged;
            this.specialValues.SpecialValueRemoved -= SpecialValueRemoved;
            this.specialValues.Dispose();
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    this.Answer = null;
                    this.previousAnswer = null;
                    this.specialValues.ClearSelectionAndShowValues();
                }
            }
        }

        public IObservableCollection<ICompositeEntity> Children => specialValues.AsChildren;
    }
}