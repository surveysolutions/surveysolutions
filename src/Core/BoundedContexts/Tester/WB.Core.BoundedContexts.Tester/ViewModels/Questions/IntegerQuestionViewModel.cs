using System;
using System.Globalization;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.Tester.ViewModels.Questions
{
    public class IntegerQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        const int RosterUpperBoundDefaultValue = 40;

        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInteraction userInteraction;
        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<NumericIntegerQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        private bool isRosterSizeQuestion;

        private int? previousAnswer;
        private int answerMaxValue;

        private string answerAsString;
        public string AnswerAsString
        {
            get { return answerAsString; }
            set
            {
                if (answerAsString != value)
                {
                    answerAsString = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand(async () => await this.SendAnswerIntegerQuestionCommand())); }
        }

        public IntegerQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<NumericIntegerQuestionAnswered> questionStateViewModel,
            IUserInteraction userInteraction,
            AnsweringViewModel answering)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.userInteraction = userInteraction;
            this.Answering = answering;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetIntegerNumericAnswer(entityIdentity);

            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = questionnaire.GetIntegerNumericQuestion(entityIdentity.Id);

            if (answerModel.IsAnswered)
            {
                var answer = answerModel.Answer;
                this.AnswerAsString = NullableIntToAnswerString(answer);
                this.previousAnswer = Monads.Maybe(() => answer);
            }
            this.isRosterSizeQuestion = questionModel.IsRosterSizeQuestion;
            this.answerMaxValue = questionModel.MaxValue ?? RosterUpperBoundDefaultValue;
        }

        private async Task SendAnswerIntegerQuestionCommand()
        {
            if (string.IsNullOrWhiteSpace(AnswerAsString))
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Integer_EmptyValueError);
                return;
            }

            int answer;
            if (!int.TryParse(this.AnswerAsString, NumberStyles.Any, CultureInfo.InvariantCulture, out answer))
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Integer_ParsingError);
                return;
            }

            if (isRosterSizeQuestion)
            {
                if (answer < 0)
                {
                    var message = string.Format(UIResources.Interview_Question_Integer_NegativeRosterSizeAnswer, AnswerAsString);
                    this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                    return;
                }

                if (answer > this.answerMaxValue)
                {
                    var message = string.Format(UIResources.Interview_Question_Integer_RosterSizeAnswerMoreThanMaxValue, AnswerAsString, this.answerMaxValue);
                    this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                    return;
                }

                if (previousAnswer.HasValue && answer < previousAnswer)
                {
                    var amountOfRostersToRemove = previousAnswer - answer;
                    var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                    if (!(await userInteraction.ConfirmAsync(message)))
                    {
                        AnswerAsString = NullableIntToAnswerString(previousAnswer);
                        return;
                    }
                }
            }

            var command = new AnswerNumericIntegerQuestionCommand(
                interviewId: Guid.Parse(interviewId),
                userId: principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answer: answer);

            try
            {
                await this.Answering.SendAnswerQuestionCommand(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();

                this.previousAnswer = answer;
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private static string NullableIntToAnswerString(int? answer)
        {
            return answer.HasValue ? answer.Value.ToString(CultureInfo.InvariantCulture) : null;
        }
    }
}