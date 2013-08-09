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

        private void Apply(AnswerCommented @event) {}

        private void Apply(FlagSetToAnswer @event) {}

        private void Apply(FlagRemovedFromAnswer @event) {}

        private void Apply(GroupPropagated @event) {}

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
            ThrowIfSomeQuestionsHaveInvalidCustomValidationExpressions(questionnaire, questionnaireId);
            ThrowIfSomeGroupsHaveInvalidCustomEnablementConditions(questionnaire, questionnaireId);
            ThrowIfSomeQuestionsHaveInvalidCustomEnablementConditions(questionnaire, questionnaireId);

            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
        }


        public void AnswerTextQuestion(Guid userId, Guid questionId, DateTime answerTime, string answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.Text);
            this.ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, questionId);

            List<Guid> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(questionId, answer, questionnaire,
                out answersDeclaredValid, out answersDeclaredInvalid);

            List<Guid> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateForGroupsWhichDependOnQuestionBeingAnswered(questionId, answer, questionnaire,
                out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateForQuestionsWhichDependOnQuestionBeingAnswered(questionId, answer, questionnaire,
                out questionsToBeDisabled, out questionsToBeEnabled);


            this.ApplyEvent(new TextQuestionAnswered(userId, questionId, answerTime, answer));

            answersDeclaredValid.ForEach(validQuestionId => this.ApplyEvent(new AnswerDeclaredValid(validQuestionId)));
            answersDeclaredInvalid.ForEach(invalidQuestionId => this.ApplyEvent(new AnswerDeclaredInvalid(invalidQuestionId)));

            groupsToBeDisabled.ForEach(disabledGroupId => this.ApplyEvent(new GroupDisabled(disabledGroupId)));
            groupsToBeEnabled.ForEach(enabledGroupId => this.ApplyEvent(new GroupEnabled(enabledGroupId)));
            questionsToBeDisabled.ForEach(disabledQuestionId => this.ApplyEvent(new QuestionDisabled(disabledQuestionId)));
            questionsToBeEnabled.ForEach(enabledQuestionId => this.ApplyEvent(new QuestionEnabled(enabledQuestionId)));
        }

        public void AnswerNumericQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.AutoPropagate, QuestionType.Numeric);
            this.ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, questionId);

            if (questionnaire.ShouldQuestionPropagateGroups(questionId))
            {
                ThrowIfAnswerCannotBeUsedAsPropagationCount(questionnaire, questionId, answer);
            }


            List<Guid> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(questionId, answer, questionnaire,
                out answersDeclaredValid, out answersDeclaredInvalid);

            List<Guid> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateForGroupsWhichDependOnQuestionBeingAnswered(questionId, answer, questionnaire,
                out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateForQuestionsWhichDependOnQuestionBeingAnswered(questionId, answer, questionnaire,
                out questionsToBeDisabled, out questionsToBeEnabled);

            List<Guid> groupsToBePropagated = questionnaire.GetGroupsPropagatedByQuestion(questionId).ToList();
            int propagationCount = groupsToBePropagated.Any() ? ToPropagationCount(answer) : 0;


            this.ApplyEvent(new NumericQuestionAnswered(userId, questionId, answerTime, answer));

            answersDeclaredValid.ForEach(validQuestionId => this.ApplyEvent(new AnswerDeclaredValid(validQuestionId)));
            answersDeclaredInvalid.ForEach(invalidQuestionId => this.ApplyEvent(new AnswerDeclaredInvalid(invalidQuestionId)));

            groupsToBeDisabled.ForEach(disabledGroupId => this.ApplyEvent(new GroupDisabled(disabledGroupId)));
            groupsToBeEnabled.ForEach(enabledGroupId => this.ApplyEvent(new GroupEnabled(enabledGroupId)));
            questionsToBeDisabled.ForEach(disabledQuestionId => this.ApplyEvent(new QuestionDisabled(disabledQuestionId)));
            questionsToBeEnabled.ForEach(enabledQuestionId => this.ApplyEvent(new QuestionEnabled(enabledQuestionId)));

            groupsToBePropagated.ForEach(propagatedGroupId => this.ApplyEvent(new GroupPropagated(propagatedGroupId, propagationCount)));
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, DateTime answerTime, DateTime answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.DateTime);
            this.ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, questionId);

            List<Guid> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(questionId, answer, questionnaire,
                out answersDeclaredValid, out answersDeclaredInvalid);

            List<Guid> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateForGroupsWhichDependOnQuestionBeingAnswered(questionId, answer, questionnaire,
                out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateForQuestionsWhichDependOnQuestionBeingAnswered(questionId, answer, questionnaire,
                out questionsToBeDisabled, out questionsToBeEnabled);


            this.ApplyEvent(new DateTimeQuestionAnswered(userId, questionId, answerTime, answer));

            answersDeclaredValid.ForEach(validQuestionId => this.ApplyEvent(new AnswerDeclaredValid(validQuestionId)));
            answersDeclaredInvalid.ForEach(invalidQuestionId => this.ApplyEvent(new AnswerDeclaredInvalid(invalidQuestionId)));

            groupsToBeDisabled.ForEach(disabledGroupId => this.ApplyEvent(new GroupDisabled(disabledGroupId)));
            groupsToBeEnabled.ForEach(enabledGroupId => this.ApplyEvent(new GroupEnabled(enabledGroupId)));
            questionsToBeDisabled.ForEach(disabledQuestionId => this.ApplyEvent(new QuestionDisabled(disabledQuestionId)));
            questionsToBeEnabled.ForEach(enabledQuestionId => this.ApplyEvent(new QuestionEnabled(enabledQuestionId)));
        }

        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal selectedValue)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.SingleOption);
            ThrowIfValueIsNotOneOfAvailableOptions(questionnaire, questionId, selectedValue);
            this.ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, questionId);

            List<Guid> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(questionId, selectedValue, questionnaire,
                out answersDeclaredValid, out answersDeclaredInvalid);

            List<Guid> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateForGroupsWhichDependOnQuestionBeingAnswered(questionId, selectedValue, questionnaire,
                out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateForQuestionsWhichDependOnQuestionBeingAnswered(questionId, selectedValue, questionnaire,
                out questionsToBeDisabled, out questionsToBeEnabled);


            this.ApplyEvent(new SingleOptionQuestionAnswered(userId, questionId, answerTime, selectedValue));

            answersDeclaredValid.ForEach(validQuestionId => this.ApplyEvent(new AnswerDeclaredValid(validQuestionId)));
            answersDeclaredInvalid.ForEach(invalidQuestionId => this.ApplyEvent(new AnswerDeclaredInvalid(invalidQuestionId)));

            groupsToBeDisabled.ForEach(disabledGroupId => this.ApplyEvent(new GroupDisabled(disabledGroupId)));
            groupsToBeEnabled.ForEach(enabledGroupId => this.ApplyEvent(new GroupEnabled(enabledGroupId)));
            questionsToBeDisabled.ForEach(disabledQuestionId => this.ApplyEvent(new QuestionDisabled(disabledQuestionId)));
            questionsToBeEnabled.ForEach(enabledQuestionId => this.ApplyEvent(new QuestionEnabled(enabledQuestionId)));
        }

        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal[] selectedValues)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.MultyOption);
            ThrowIfSomeValuesAreNotFromAvailableOptions(questionnaire, questionId, selectedValues);
            this.ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, questionId);

            List<Guid> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(questionId, selectedValues, questionnaire,
                out answersDeclaredValid, out answersDeclaredInvalid);

            List<Guid> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateForGroupsWhichDependOnQuestionBeingAnswered(questionId, selectedValues, questionnaire,
                out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateForQuestionsWhichDependOnQuestionBeingAnswered(questionId, selectedValues, questionnaire,
                out questionsToBeDisabled, out questionsToBeEnabled);


            this.ApplyEvent(new MultipleOptionsQuestionAnswered(userId, questionId, answerTime, selectedValues));

            answersDeclaredValid.ForEach(validQuestionId => this.ApplyEvent(new AnswerDeclaredValid(validQuestionId)));
            answersDeclaredInvalid.ForEach(invalidQuestionId => this.ApplyEvent(new AnswerDeclaredInvalid(invalidQuestionId)));

            groupsToBeDisabled.ForEach(disabledGroupId => this.ApplyEvent(new GroupDisabled(disabledGroupId)));
            groupsToBeEnabled.ForEach(enabledGroupId => this.ApplyEvent(new GroupEnabled(enabledGroupId)));
            questionsToBeDisabled.ForEach(disabledQuestionId => this.ApplyEvent(new QuestionDisabled(disabledQuestionId)));
            questionsToBeEnabled.ForEach(enabledQuestionId => this.ApplyEvent(new QuestionEnabled(enabledQuestionId)));
        }

        public void CommentAnswer(Guid userId, Guid questionId, string comment)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);

            this.ApplyEvent(new AnswerCommented(userId, questionId, comment));
        }

        public void SetFlagToAnswer(Guid userId, Guid questionId)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);

            this.ApplyEvent(new FlagSetToAnswer(userId, questionId));
        }

        public void RemoveFlagFromAnswer(Guid userId, Guid questionId)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);

            this.ApplyEvent(new FlagRemovedFromAnswer(userId, questionId));
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

        private void ThrowIfQuestionDoesNotExist(IQuestionnaire questionnaire, Guid questionId)
        {
            if (!questionnaire.HasQuestion(questionId))
                throw new InterviewException(string.Format("Question with id '{0}' is not found.", questionId));
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

        private static void ThrowIfValueIsNotOneOfAvailableOptions(IQuestionnaire questionnaire, Guid questionId, decimal value)
        {
            IEnumerable<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId);

            bool valueIsNotOneOfAvailable = !availableValues.Contains(value);
            if (valueIsNotOneOfAvailable)
                throw new InterviewException(string.Format(
                    "For question with id '{0}' was provided selected value {1} as answer. But only following values are allowed: {2}.",
                    questionId, value, JoinDecimalsWithComma(availableValues)));
        }

        private static void ThrowIfSomeValuesAreNotFromAvailableOptions(IQuestionnaire questionnaire, Guid questionId, decimal[] values)
        {
            IEnumerable<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId);

            bool someValueIsNotOneOfAvailable = values.Any(value => !availableValues.Contains(value));
            if (someValueIsNotOneOfAvailable)
                throw new InterviewException(string.Format(
                    "For question with id '{0}' were provided selected values {1} as answer. But only following values are allowed: {2}.",
                    questionId, JoinDecimalsWithComma(values), JoinDecimalsWithComma(availableValues)));
        }

        private static void ThrowIfSomeQuestionsHaveInvalidCustomValidationExpressions(IQuestionnaire questionnaire, Guid questionnaireId)
        {
            IEnumerable<Guid> invalidQuestions = questionnaire.GetQuestionsWithInvalidCustomValidationExpressions();

            if (invalidQuestions.Any())
                throw new InterviewException(string.Format(
                    "Cannot create interview from questionnaire '{1}' because following questions in it have invalid validation expressions:{0}{2}",
                    Environment.NewLine, questionnaireId,
                    string.Join(
                        Environment.NewLine,
                        invalidQuestions.Select(questionId
                            => string.Format("{0} : {1}", questionId, questionnaire.GetCustomValidationExpression(questionId))))));
        }

        private static void ThrowIfSomeGroupsHaveInvalidCustomEnablementConditions(IQuestionnaire questionnaire, Guid questionnaireId)
        {
            IEnumerable<Guid> invalidGroups = questionnaire.GetGroupsWithInvalidCustomEnablementConditions();

            if (invalidGroups.Any())
                throw new InterviewException(string.Format(
                    "Cannot create interview from questionnaire '{1}' because following groups in it have invalid enablement conditions:{0}{2}",
                    Environment.NewLine, questionnaireId,
                    string.Join(
                        Environment.NewLine,
                        invalidGroups.Select(groupId
                            => string.Format("{0} : {1}", groupId, questionnaire.GetCustomEnablementConditionForGroup(groupId))))));
        }

        private static void ThrowIfSomeQuestionsHaveInvalidCustomEnablementConditions(IQuestionnaire questionnaire, Guid questionnaireId)
        {
            IEnumerable<Guid> invalidQuestions = questionnaire.GetQuestionsWithInvalidCustomEnablementConditions();

            if (invalidQuestions.Any())
                throw new InterviewException(string.Format(
                    "Cannot create interview from questionnaire '{1}' because following questions in it have invalid enablement conditions:{0}{2}",
                    Environment.NewLine, questionnaireId,
                    string.Join(
                        Environment.NewLine,
                        invalidQuestions.Select(questionId
                            => string.Format("{0} : {1}", questionId, questionnaire.GetCustomEnablementConditionForQuestion(questionId))))));
        }

        private void ThrowIfQuestionOrParentGroupIsDisabled(IQuestionnaire questionnaire, Guid questionId)
        {
            if (this.IsQuestionDisabled(questionId))
                throw new InterviewException(string.Format(
                    "Question '{1}' is disabled by it's following enablement condition:{0}{2}",
                    Environment.NewLine, questionId, questionnaire.GetCustomEnablementConditionForQuestion(questionId)));

            IEnumerable<Guid> parentGroups = questionnaire.GetAllParentGroupsForQuestion(questionId);
            foreach (Guid parentGroupId in parentGroups)
            {
                if (this.IsGroupDisabled(parentGroupId))
                    throw new InterviewException(string.Format(
                        "Question '{1}' is disabled because parent group '{2}' is disabled by it's following enablement condition:{0}{3}",
                        Environment.NewLine, questionId, parentGroupId, questionnaire.GetCustomEnablementConditionForGroup(parentGroupId)));
            }
        }

        private static void ThrowIfAnswerCannotBeUsedAsPropagationCount(IQuestionnaire questionnaire, Guid questionId, decimal answer)
        {
            int maxValue = questionnaire.GetMaxAnswerValueForPropagatingQuestion(questionId);

            bool answerIsNotInteger = answer != (int) answer;
            bool answerIsNegative = answer < 0;
            bool answerExceedsMaxValue = answer > maxValue;

            if (answerIsNotInteger)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question with id '{1}' is incorrect because question should propagate groups and answer is not a valid integer.",
                    answer, questionId));

            if (answerIsNegative)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question with id '{1}' is incorrect because question should propagate groups and answer is negative.",
                    answer, questionId));

            if (answerExceedsMaxValue)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question with id '{1}' is incorrect because question should propagate groups and answer is greater than max value '{2}'.",
                    answer, questionId, maxValue));
        }


        private void PerformCustomValidationOfQuestionBeingAnsweredAndDependentQuestions(
            Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered, IQuestionnaire questionnaire,
            out List<Guid> answersDeclaredValid, out List<Guid> answersDeclaredInvalid)
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
                ? new List<Guid> { questionBeingAnsweredId }
                : new List<Guid>();
            answersDeclaredValid.AddRange(dependentAnswersDeclaredValid);

            answersDeclaredInvalid = wasCurrentAnswerDeclaredInvalid
                ? new List<Guid> { questionBeingAnsweredId }
                : new List<Guid>();
            answersDeclaredInvalid.AddRange(dependentAnswersDeclaredInvalid);
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

            IEnumerable<Guid> involvedQuestions = questionnaire.GetQuestionsInvolvedInCustomValidation(questionToValidateId);

            bool someOfNeededAnswersAreNotDefined
                = involvedQuestions.Any(questionId => !this.IsAnswerDefined(questionId) && questionId != questionBeingAnsweredId);

            if (someOfNeededAnswersAreNotDefined)
                return null;

            Dictionary<Guid, object> involvedAnswers
                = this.GetAnswersForSpecifiedQuestionsUsingSeparateValueForQuestionWhichIsBeingAnswered(
                    involvedQuestions, questionBeingAnsweredId, answerGivenForQuestionBeingAnswered);

            string validationExpression = questionnaire.GetCustomValidationExpression(questionToValidateId);

            return this.EvaluateValidationExpression(validationExpression, questionBeingAnsweredId, involvedAnswers);
        }


        private void DetermineCustomEnablementStateForGroupsWhichDependOnQuestionBeingAnswered(
            Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered, IQuestionnaire questionnaire,
            out List<Guid> groupsToBeDisabled, out List<Guid> groupsToBeEnabled)
        {
            groupsToBeDisabled = new List<Guid>();
            groupsToBeEnabled = new List<Guid>();

            IEnumerable<Guid> dependentGroups = questionnaire.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionBeingAnsweredId);

            foreach (Guid dependentGroupId in dependentGroups)
            {
                bool? enablementState = this.DetermineCustomEnablementStateOfGroup(
                    dependentGroupId, questionnaire, questionBeingAnsweredId, answerGivenForQuestionBeingAnswered);

                bool shouldGroupBeDisabled = enablementState == false;
                bool shouldGroupBeEnabled = enablementState == true;

                if (shouldGroupBeDisabled && !this.IsGroupDisabled(dependentGroupId))
                {
                    groupsToBeDisabled.Add(dependentGroupId);
                }

                if (shouldGroupBeEnabled && this.IsGroupDisabled(dependentGroupId))
                {
                    groupsToBeEnabled.Add(dependentGroupId);
                }
            }
        }

        private void DetermineCustomEnablementStateForQuestionsWhichDependOnQuestionBeingAnswered(
            Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered, IQuestionnaire questionnaire,
            out List<Guid> questionsToBeDisabled, out List<Guid> questionsToBeEnabled)
        {
            questionsToBeDisabled = new List<Guid>();
            questionsToBeEnabled = new List<Guid>();

            IEnumerable<Guid> dependentQuestions = questionnaire.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionBeingAnsweredId);

            foreach (Guid dependentQuestionId in dependentQuestions)
            {
                bool? enablementState = this.DetermineCustomEnablementStateOfQuestion(
                    dependentQuestionId, questionnaire, questionBeingAnsweredId, answerGivenForQuestionBeingAnswered);

                bool shouldQuestionBeDisabled = enablementState == false;
                bool shouldQuestionBeEnabled = enablementState == true;

                if (shouldQuestionBeDisabled && !this.IsQuestionDisabled(dependentQuestionId))
                {
                    questionsToBeDisabled.Add(dependentQuestionId);
                }

                if (shouldQuestionBeEnabled && this.IsQuestionDisabled(dependentQuestionId))
                {
                    questionsToBeEnabled.Add(dependentQuestionId);
                }
            }
        }

        private bool? DetermineCustomEnablementStateOfGroup(Guid groupId,
            IQuestionnaire questionnaire, Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered)
        {
            IEnumerable<Guid> involvedQuestions = questionnaire.GetQuestionsInvolvedInCustomEnablementConditionForGroup(groupId);

            string enablementCondition = questionnaire.GetCustomEnablementConditionForGroup(groupId);

            return this.DetermineCustomEnablementState(enablementCondition,
                involvedQuestions, questionBeingAnsweredId, answerGivenForQuestionBeingAnswered);
        }

        private bool? DetermineCustomEnablementStateOfQuestion(Guid questionId,
            IQuestionnaire questionnaire, Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered)
        {
            IEnumerable<Guid> involvedQuestions = questionnaire.GetQuestionsInvolvedInCustomEnablementConditionForQuestion(questionId);

            string enablementCondition = questionnaire.GetCustomEnablementConditionForQuestion(questionId);

            return this.DetermineCustomEnablementState(enablementCondition,
                involvedQuestions, questionBeingAnsweredId, answerGivenForQuestionBeingAnswered);
        }

        private bool? DetermineCustomEnablementState(string enablementCondition, IEnumerable<Guid> involvedQuestions,
            Guid questionBeingAnsweredId, object answerGivenForQuestionBeingAnswered)
        {
            bool someOfNeededAnswersAreNotDefined
                = involvedQuestions.Any(questionId => !this.IsAnswerDefined(questionId) && questionId != questionBeingAnsweredId);

            if (someOfNeededAnswersAreNotDefined)
                return null;

            Dictionary<Guid, object> involvedAnswers
                = this.GetAnswersForSpecifiedQuestionsUsingSeparateValueForQuestionWhichIsBeingAnswered(
                    involvedQuestions, questionBeingAnsweredId, answerGivenForQuestionBeingAnswered);

            return this.EvaluateEnablementCondition(enablementCondition, involvedAnswers);
        }


        private bool EvaluateValidationExpression(string validationExpression, Guid contextQuestionId, Dictionary<Guid, object> involvedAnswers)
        {
            return this.ExpressionProcessor.EvaluateBooleanExpression(validationExpression,
                getValueForIdentifier: identifier => involvedAnswers[GetQuestionIdByExpressionIdentifierIncludingThis(identifier, contextQuestionId)]);
        }

        private bool EvaluateEnablementCondition(string enablementCondition, Dictionary<Guid, object> involvedAnswers)
        {
            return this.ExpressionProcessor.EvaluateBooleanExpression(enablementCondition,
                getValueForIdentifier: identifier => involvedAnswers[GetQuestionIdByExpressionIdentifierExcludingThis(identifier)]);
        }

        private static Guid GetQuestionIdByExpressionIdentifierIncludingThis(string identifier, Guid contextQuestionId)
        {
            if (identifier.ToLower() == "this")
                return contextQuestionId;

            return GetQuestionIdByExpressionIdentifierExcludingThis(identifier);
        }

        private static Guid GetQuestionIdByExpressionIdentifierExcludingThis(string identifier)
        {
            return Guid.Parse(identifier);
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

        private bool IsGroupDisabled(Guid groupId)
        {
            return this.disabledGroups.Contains(groupId);
        }

        private bool IsQuestionDisabled(Guid questionId)
        {
            return this.disabledQuestions.Contains(questionId);
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

        private static int ToPropagationCount(decimal decimalValue)
        {
            return (int) decimalValue;
        }

        private static string JoinDecimalsWithComma(IEnumerable<decimal> values)
        {
            return string.Join(", ", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
