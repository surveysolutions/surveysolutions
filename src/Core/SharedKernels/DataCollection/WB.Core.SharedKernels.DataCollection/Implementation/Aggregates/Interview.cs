using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Interview : AggregateRootMappedByConvention
    {
        #region State

        private Guid questionnaireId;
        private long questionnaireVersion;
        private readonly Dictionary<Guid, object> answers = new Dictionary<Guid, object>();

        private void Apply(InterviewCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        private void Apply(TextQuestionAnswered @event)
        {
            this.answers[@event.QuestionId] = @event.Answer;
        }

        private void Apply(NumericQuestionAnswered @event)
        {
            this.answers[@event.QuestionId] = @event.Answer;
        }

        private void Apply(DateTimeQuestionAnswered @event)
        {
            this.answers[@event.QuestionId] = @event.Answer;
        }

        private void Apply(SingleOptionQuestionAnswered @event)
        {
            this.answers[@event.QuestionId] = @event.SelectedValue;
        }

        private void Apply(MultipleOptionsQuestionAnswered @event)
        {
            this.answers[@event.QuestionId] = @event.SelectedValues;
        }

        #endregion

        #region Dependencies

        /// <remarks>
        /// Repository operations are time-consuming.
        /// So this repository may be used only in command handlers.
        /// And should never be used in event handlers!!
        /// </remarks>
        private IQuestionnaireRepository QuestionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IQuestionnaireRepository>(); }
        }

        /// <remarks>
        /// All operations with expressions are time-consuming.
        /// So this processor may be used only in command handlers.
        /// And should never be used in event handlers!!
        /// </remarks>
        private IExpressionProcessor ExpressionProcessor
        {
            get { return ServiceLocator.Current.GetInstance<IExpressionProcessor>(); }
        }

        #endregion

        /// <remarks>Is used to restore aggregate from event stream.</remarks>
        public Interview() {}

        public Interview(Guid questionnaireId, Guid userId)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireId);
            ThrowIfSomeQuestionsHaveInvalidCustomValidationExpression(questionnaire, questionnaireId);

            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
        }

        public void AnswerTextQuestion(Guid userId, Guid questionId, DateTime answerTime, string answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.Text);

            bool? customValidationResult = this.PerformCustomValidationOfQuestionBeingAnswered(questionId, answer, questionnaire);

            this.ApplyEvent(new TextQuestionAnswered(userId, questionId, answerTime, answer));

            if (customValidationResult.HasValue)
            {
                this.ApplyEvent(customValidationResult.Value
                    ? new AnswerDeclaredValid(questionId) as object
                    : new AnswerDeclaredInvalid(questionId) as object);
            }
        }

        public void AnswerNumericQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.AutoPropagate, QuestionType.Numeric);

            this.ApplyEvent(new NumericQuestionAnswered(userId, questionId, answerTime, answer));
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, DateTime answerTime, DateTime answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.DateTime);

            this.ApplyEvent(new DateTimeQuestionAnswered(userId, questionId, answerTime, answer));
        }

        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal selectedValue)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.SingleOption);
            ThrowIfValueIsNotOneOfAvailableOptions(questionnaire, questionId, selectedValue);

            this.ApplyEvent(new SingleOptionQuestionAnswered(userId, questionId, answerTime, selectedValue));
        }

        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal[] selectedValues)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.MultyOption);
            this.ThrowIfSomeValuesAreNotFromAvailableOptions(questionnaire, questionId, selectedValues);

            this.ApplyEvent(new MultipleOptionsQuestionAnswered(userId, questionId, answerTime, selectedValues));
        }

        private IQuestionnaire GetHistoricalQuestionnaireOrThrow(Guid id, long version)
        {
            IQuestionnaire questionnaire = this.QuestionnaireRepository.GetHistoricalQuestionnaire(id, version);

            if (questionnaire == null)
                throw new InterviewException(string.Format("Questionnaire with id '{0}' of version {1} is not found.", id, version));

            return questionnaire;
        }

        private IQuestionnaire GetQuestionnaireOrThrow(Guid id)
        {
            IQuestionnaire questionnaire = this.QuestionnaireRepository.GetQuestionnaire(id);

            if (questionnaire == null)
                throw new InterviewException(string.Format("Questionnaire with id '{0}' is not found.", id));

            return questionnaire;
        }

        private static void ThrowIfQuestionTypeIsNotOneOfExpected(IQuestionnaire questionnaire, Guid questionId, params QuestionType[] expectedQuestionTypes)
        {
            QuestionType questionType = questionnaire.GetQuestionType(questionId);

            bool typeIsNotExpected = !expectedQuestionTypes.Contains(questionType);
            if (typeIsNotExpected)
                throw new InterviewException(string.Format(
                    "Question with id '{0}' has type {1}. But one of the following types was expected: {2}.",
                    questionId, questionType, string.Join(", ", expectedQuestionTypes.Select(type => type.ToString()))));
        }

        private void ThrowIfValueIsNotOneOfAvailableOptions(IQuestionnaire questionnaire, Guid questionId, decimal value)
        {
            IEnumerable<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId);

            bool valueIsNotOneOfAvailable = !availableValues.Contains(value);
            if (valueIsNotOneOfAvailable)
                throw new InterviewException(string.Format(
                    "For question with id '{0}' was provided selected value {1} as answer. But only following values are allowed: {2}.",
                    questionId, value, JoinDecimalsWithComma(availableValues)));
        }

        private void ThrowIfSomeValuesAreNotFromAvailableOptions(IQuestionnaire questionnaire, Guid questionId, decimal[] values)
        {
            IEnumerable<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId);

            bool someValueIsNotOneOfAvailable = values.Any(value => !availableValues.Contains(value));
            if (someValueIsNotOneOfAvailable)
                throw new InterviewException(string.Format(
                    "For question with id '{0}' were provided selected values {1} as answer. But only following values are allowed: {2}.",
                    questionId, JoinDecimalsWithComma(values), JoinDecimalsWithComma(availableValues)));
        }

        private static void ThrowIfSomeQuestionsHaveInvalidCustomValidationExpression(IQuestionnaire questionnaire, Guid questionnaireId)
        {
            IEnumerable<Guid> questionsWithInvalidValidationExpressions = questionnaire.GetQuestionsWithInvalidCustomValidationExpressions();

            if (questionsWithInvalidValidationExpressions.Any())
                throw new InterviewException(string.Format(
                    "Cannot create interview from questionnaire '{1}' because following questions in it have invalid validation expressions:{0}{2}",
                    Environment.NewLine, questionnaireId,
                    string.Join(
                        Environment.NewLine,
                        questionsWithInvalidValidationExpressions.Select(questionId
                            => string.Format("{0} : {1}", questionId, questionnaire.GetCustomValidationExpression(questionId))))));
        }

        private bool? PerformCustomValidationOfQuestionBeingAnswered(Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered, IQuestionnaire questionnaire)
        {
            if (!questionnaire.IsCustomValidationDefined(questionBeingAnsweredId))
                return true;

            IEnumerable<Guid> questionsInvolvedInCustomValidation = questionnaire.GetQuestionsInvolvedInCustomValidation(questionBeingAnsweredId);

            bool someOfAnswersNeededForCustomValidationAreNotDefined
                = questionsInvolvedInCustomValidation
                    .Any(questionId => !this.IsAnswerDefined(questionId) && questionId != questionBeingAnsweredId);

            if (someOfAnswersNeededForCustomValidationAreNotDefined)
                return null;

            Dictionary<Guid, object> answersInvolvedInCustomValidation
                = this.GetAnswersForSpecifiedQuestionsUsingSeparateValueForQuestionWhichIsBeingAnswered(
                    questionsInvolvedInCustomValidation, questionBeingAnsweredId, answerGivenForQuestionBeingAnswered);

            string validationExpression = questionnaire.GetCustomValidationExpression(questionBeingAnsweredId);

            return this.EvaluateValidationExpression(validationExpression, questionBeingAnsweredId, answersInvolvedInCustomValidation);
        }

        private bool EvaluateValidationExpression(string expression, Guid contextQuestionId, Dictionary<Guid, object> involvedAnswers)
        {
            return this.ExpressionProcessor.EvaluateBooleanExpression(expression,
                getValueForIdentifier: identifier => involvedAnswers[MapExpressionIdentifierToQuestionId(contextQuestionId, identifier)]);
        }

        private static Guid MapExpressionIdentifierToQuestionId(Guid contextQuestionId, string identifier)
        {
            return identifier.ToLower() == "this"
                ? contextQuestionId
                : Guid.Parse(identifier);
        }

        private Dictionary<Guid, object> GetAnswersForSpecifiedQuestionsUsingSeparateValueForQuestionWhichIsBeingAnswered(
            IEnumerable<Guid> questions, Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered)
        {
            Dictionary<Guid, object> answersForSpecifiedQuestions = this.GetAnswersForSpecifiedQuestions(questions);

            if (answersForSpecifiedQuestions.ContainsKey(questionBeingAnsweredId))
                answersForSpecifiedQuestions[questionBeingAnsweredId] = answerGivenForQuestionBeingAnswered;

            return answersForSpecifiedQuestions;
        }

        private Dictionary<Guid, object> GetAnswersForSpecifiedQuestions(IEnumerable<Guid> questions)
        {
            return questions.ToDictionary(
                questionId => questionId,
                questionId => this.GetAnswerForQuestionOrThrow(questionId));
        }

        private object GetAnswerForQuestionOrThrow(Guid questionId)
        {
            if (!this.IsAnswerDefined(questionId))
                throw new InterviewException(string.Format(
                    "Cannot get answer for question with id '{0}' because it was not yet answered.",
                    questionId));

            return this.answers[questionId];
        }

        private bool IsAnswerDefined(Guid questionId)
        {
            return this.answers.ContainsKey(questionId);
        }

        private static string JoinDecimalsWithComma(IEnumerable<decimal> values)
        {
            return string.Join(", ", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
