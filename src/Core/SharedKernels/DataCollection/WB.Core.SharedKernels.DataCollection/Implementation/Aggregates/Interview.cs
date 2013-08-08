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
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
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
        private readonly HashSet<Guid> disabledGroups = new HashSet<Guid>();
        private readonly HashSet<Guid> disabledQuestions = new HashSet<Guid>();

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

        private void Apply(AnswerDeclaredValid @event) {}

        private void Apply(AnswerDeclaredInvalid @event) {}

        private void Apply(GroupDisabled @event)
        {
            this.disabledGroups.Add(@event.GroupId);
        }

        private void Apply(GroupEnabled @event)
        {
            this.disabledGroups.Remove(@event.GroupId);
        }

        private void Apply(QuestionDisabled @event)
        {
            this.disabledGroups.Add(@event.QuestionId);
        }

        private void Apply(QuestionEnabled @event)
        {
            this.disabledGroups.Remove(@event.QuestionId);
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

        public Interview(InterviewSynchronizationDto sycnhronizedInterview)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(sycnhronizedInterview.QuestionnaireId);
            ThrowIfSomeQuestionsHaveInvalidCustomValidationExpression(questionnaire, questionnaireId);
            this.ApplyEvent(new InterviewSynchronized(sycnhronizedInterview.UserId,
                                                      sycnhronizedInterview.QuestionnaireId,
                                                      sycnhronizedInterview.StatusId, questionnaire.Version,
                                                      sycnhronizedInterview.Answers));
        }


        public void AnswerTextQuestion(Guid userId, Guid questionId, DateTime answerTime, string answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.Text);
            ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, questionId);

            IEnumerable<Guid> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(questionId, answer, questionnaire,
                out answersDeclaredValid, out answersDeclaredInvalid);


            this.ApplyEvent(new TextQuestionAnswered(userId, questionId, answerTime, answer));

            foreach (Guid answerDeclaredValidId in answersDeclaredValid)
            {
                this.ApplyEvent(new AnswerDeclaredValid(answerDeclaredValidId));
            }

            foreach (Guid answerDeclaredInvalidId in answersDeclaredInvalid)
            {
                this.ApplyEvent(new AnswerDeclaredInvalid(answerDeclaredInvalidId));
            }
        }

        public void AnswerNumericQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.AutoPropagate, QuestionType.Numeric);
            ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, questionId);

            IEnumerable<Guid> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(questionId, answer, questionnaire,
                out answersDeclaredValid, out answersDeclaredInvalid);


            this.ApplyEvent(new NumericQuestionAnswered(userId, questionId, answerTime, answer));

            foreach (Guid answerDeclaredValidId in answersDeclaredValid)
            {
                this.ApplyEvent(new AnswerDeclaredValid(answerDeclaredValidId));
            }

            foreach (Guid answerDeclaredInvalidId in answersDeclaredInvalid)
            {
                this.ApplyEvent(new AnswerDeclaredInvalid(answerDeclaredInvalidId));
            }
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, DateTime answerTime, DateTime answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.DateTime);
            ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, questionId);

            IEnumerable<Guid> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(questionId, answer, questionnaire,
                out answersDeclaredValid, out answersDeclaredInvalid);


            this.ApplyEvent(new DateTimeQuestionAnswered(userId, questionId, answerTime, answer));

            foreach (Guid answerDeclaredValidId in answersDeclaredValid)
            {
                this.ApplyEvent(new AnswerDeclaredValid(answerDeclaredValidId));
            }

            foreach (Guid answerDeclaredInvalidId in answersDeclaredInvalid)
            {
                this.ApplyEvent(new AnswerDeclaredInvalid(answerDeclaredInvalidId));
            }
        }

        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal selectedValue)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.SingleOption);
            ThrowIfValueIsNotOneOfAvailableOptions(questionnaire, questionId, selectedValue);
            ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, questionId);

            IEnumerable<Guid> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(questionId, selectedValue, questionnaire,
                out answersDeclaredValid, out answersDeclaredInvalid);


            this.ApplyEvent(new SingleOptionQuestionAnswered(userId, questionId, answerTime, selectedValue));

            foreach (Guid answerDeclaredValidId in answersDeclaredValid)
            {
                this.ApplyEvent(new AnswerDeclaredValid(answerDeclaredValidId));
            }

            foreach (Guid answerDeclaredInvalidId in answersDeclaredInvalid)
            {
                this.ApplyEvent(new AnswerDeclaredInvalid(answerDeclaredInvalidId));
            }
        }

        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal[] selectedValues)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.MultyOption);
            this.ThrowIfSomeValuesAreNotFromAvailableOptions(questionnaire, questionId, selectedValues);
            ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, questionId);

            IEnumerable<Guid> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(questionId, selectedValues, questionnaire,
                out answersDeclaredValid, out answersDeclaredInvalid);


            this.ApplyEvent(new MultipleOptionsQuestionAnswered(userId, questionId, answerTime, selectedValues));

            foreach (Guid answerDeclaredValidId in answersDeclaredValid)
            {
                this.ApplyEvent(new AnswerDeclaredValid(answerDeclaredValidId));
            }

            foreach (Guid answerDeclaredInvalidId in answersDeclaredInvalid)
            {
                this.ApplyEvent(new AnswerDeclaredInvalid(answerDeclaredInvalidId));
            }
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

        private void ThrowIfQuestionOrParentGroupIsDisabled(IQuestionnaire questionnaire, Guid questionId)
        {
            bool questionIsDisabled = this.disabledQuestions.Contains(questionId);
            if (questionIsDisabled)
                throw new InterviewException(string.Format(
                    "Question '{1}' is disabled by it's following enablement condition:{0}{2}",
                    Environment.NewLine, questionId, questionnaire.GetCustomEnablementConditionForQuestion(questionId)));

            IEnumerable<Guid> parentGroups = questionnaire.GetAllParentGroupsForQuestion(questionId);
            foreach (Guid parentGroupId in parentGroups)
            {
                bool groupIsDisabled = this.disabledGroups.Contains(parentGroupId);
                if (groupIsDisabled)
                    throw new InterviewException(string.Format(
                        "Question '{1}' is disabled because parent group '{2}' is disabled by it's following enablement condition:{0}{3}",
                        Environment.NewLine, questionId, parentGroupId, questionnaire.GetCustomEnablementConditionForGroup(parentGroupId)));
            }
        }

        private void PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(
            Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered, IQuestionnaire questionnaire,
            out IEnumerable<Guid> answersDeclaredValid, out IEnumerable<Guid> answersDeclaredInvalid)
        {
            bool? currentAnswerValidationResult
                = this.PerformCustomValidationOfQuestionBeingAnswered(questionBeingAnsweredId, answerGivenForQuestionBeingAnswered, questionnaire);

            bool wasCurrentAnswerDeclaredValid = currentAnswerValidationResult == true;
            bool wasCurrentAnswerDeclaredInvalid = currentAnswerValidationResult == false;

            IEnumerable<Guid> dependentAnswersDeclaredValid;
            IEnumerable<Guid> dependentAnswersDeclaredInvalid;
            this.PerformCustomValidationOfQuestionsWhichDependOnQuestionBeingAnswered(questionnaire, questionBeingAnsweredId, answerGivenForQuestionBeingAnswered,
                out dependentAnswersDeclaredValid, out dependentAnswersDeclaredInvalid);

            answersDeclaredValid = wasCurrentAnswerDeclaredValid
                ? Enumerable.Concat(new[] {questionBeingAnsweredId}, dependentAnswersDeclaredValid)
                : dependentAnswersDeclaredValid;

            answersDeclaredInvalid = wasCurrentAnswerDeclaredInvalid
                ? Enumerable.Concat(new[] {questionBeingAnsweredId}, dependentAnswersDeclaredInvalid)
                : dependentAnswersDeclaredInvalid;
        }

        private bool? PerformCustomValidationOfQuestionBeingAnswered(
            Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered, IQuestionnaire questionnaire)
        {
            return this.PerformCustomValidationOfQuestion(
                questionBeingAnsweredId, questionnaire, questionBeingAnsweredId, answerGivenForQuestionBeingAnswered);
        }

        private void PerformCustomValidationOfQuestionsWhichDependOnQuestionBeingAnswered(
            IQuestionnaire questionnaire, Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered,
            out IEnumerable<Guid> dependentAnswersDeclaredValid, out IEnumerable<Guid> dependentAnswersDeclaredInvalid)
        {
            var validAnswers = new List<Guid>();
            var invalidAnswers = new List<Guid>();

            IEnumerable<Guid> dependentQuestions = questionnaire.GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(questionBeingAnsweredId);

            foreach (Guid dependentQuestionId in dependentQuestions)
            {
                bool? validationResult = this.PerformCustomValidationOfQuestion(
                    dependentQuestionId, questionnaire, questionBeingAnsweredId, answerGivenForQuestionBeingAnswered);

                if (validationResult.HasValue)
                {
                    if (validationResult.Value)
                    {
                        validAnswers.Add(dependentQuestionId);
                    }
                    else
                    {
                        invalidAnswers.Add(dependentQuestionId);
                    }
                }
            }

            dependentAnswersDeclaredValid = validAnswers;
            dependentAnswersDeclaredInvalid = invalidAnswers;
        }

        private bool? PerformCustomValidationOfQuestion(Guid questionToValidateId,
            IQuestionnaire questionnaire, Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered)
        {
            if (!questionnaire.IsCustomValidationDefined(questionToValidateId))
                return true;

            IEnumerable<Guid> questionsInvolvedInCustomValidation = questionnaire.GetQuestionsInvolvedInCustomValidation(questionToValidateId);

            bool someOfAnswersNeededForCustomValidationAreNotDefined
                = questionsInvolvedInCustomValidation
                    .Any(questionId => !this.IsAnswerDefined(questionId) && questionId != questionBeingAnsweredId);

            if (someOfAnswersNeededForCustomValidationAreNotDefined)
                return null;

            Dictionary<Guid, object> answersInvolvedInCustomValidation
                = this.GetAnswersForSpecifiedQuestionsUsingSeparateValueForQuestionWhichIsBeingAnswered(
                    questionsInvolvedInCustomValidation, questionBeingAnsweredId, answerGivenForQuestionBeingAnswered);

            string validationExpression = questionnaire.GetCustomValidationExpression(questionToValidateId);

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
