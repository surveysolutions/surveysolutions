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
        private readonly Dictionary<string, object> answers = new Dictionary<string, object>();
        private readonly HashSet<string> disabledGroups = new HashSet<string>();
        private readonly HashSet<string> disabledQuestions = new HashSet<string>();
        private readonly Dictionary<string, int> propagatedGroupInstanceCounts = new Dictionary<string, int>();

        private void Apply(InterviewCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        private void Apply(TextQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answers[questionKey] = @event.Answer;
        }

        private void Apply(NumericQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answers[questionKey] = @event.Answer;
        }

        private void Apply(DateTimeQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answers[questionKey] = @event.Answer;
        }

        private void Apply(SingleOptionQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answers[questionKey] = @event.SelectedValue;
        }

        private void Apply(MultipleOptionsQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answers[questionKey] = @event.SelectedValues;
        }

        private void Apply(AnswerDeclaredValid @event) {}

        private void Apply(AnswerDeclaredInvalid @event) {}

        private void Apply(GroupDisabled @event)
        {
            string groupKey = ConvertIdAndPropagationVectorToString(@event.GroupId, @event.PropagationVector);

            this.disabledGroups.Add(groupKey);
        }

        private void Apply(GroupEnabled @event)
        {
            string groupKey = ConvertIdAndPropagationVectorToString(@event.GroupId, @event.PropagationVector);

            this.disabledGroups.Remove(groupKey);
        }

        private void Apply(QuestionDisabled @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.disabledQuestions.Add(questionKey);
        }

        private void Apply(QuestionEnabled @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.disabledQuestions.Remove(questionKey);
        }

        private void Apply(AnswerCommented @event) {}

        private void Apply(FlagSetToAnswer @event) {}

        private void Apply(FlagRemovedFromAnswer @event) {}

        private void Apply(GroupPropagated @event)
        {
            string propagatableGroupKey = ConvertIdAndPropagationVectorToString(@event.GroupId, @event.OuterScopePropagationVector);

            this.propagatedGroupInstanceCounts[propagatableGroupKey] = @event.Count;
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

        #region Types

        /// <summary>
        /// Full identity of group or question: id and propagation vector.
        /// </summary>
        /// <remarks>
        /// Is used only internally to simplify return of id and propagation vector as return value
        /// and to reduce parameters count in calculation methods.
        /// Should not be made public or be used in any form in events or commands.
        /// </remarks>
        private class Identity
        {
            public Guid Id { get; private set; }
            public int[] PropagationVector { get; private set; }

            public Identity(Guid id, int[] propagationVector)
            {
                this.Id = id;
                this.PropagationVector = propagationVector;
            }
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
            ThrowIfSomePropagatingQuestionsReferToNotExistingOrNotPropagatableGroups(questionnaire, questionnaireId);

            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
        }


        public void AnswerTextQuestion(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, string answer)
        {
            var answeredQuestion = new Identity(questionId, propagationVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            this.ThrowIfPropagationVectorIsIncorrect(questionnaire, questionId, propagationVector);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.Text);
            this.ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, answeredQuestion);


            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? answer : this.GetAnswerOrNull(question);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out answersDeclaredValid, out answersDeclaredInvalid);

            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out questionsToBeDisabled, out questionsToBeEnabled);


            this.ApplyEvent(new TextQuestionAnswered(userId, questionId, propagationVector, answerTime, answer));

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.PropagationVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.PropagationVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.PropagationVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.PropagationVector)));
            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.PropagationVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.PropagationVector)));
        }

        public void AnswerNumericQuestion(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, decimal answer)
        {
            var answeredQuestion = new Identity(questionId, propagationVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            this.ThrowIfPropagationVectorIsIncorrect(questionnaire, questionId, propagationVector);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.AutoPropagate, QuestionType.Numeric);
            this.ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, answeredQuestion);

            if (questionnaire.ShouldQuestionPropagateGroups(questionId))
            {
                ThrowIfAnswerCannotBeUsedAsPropagationCount(questionnaire, questionId, answer);
            }


            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? answer : this.GetAnswerOrNull(question);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out answersDeclaredValid, out answersDeclaredInvalid);

            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out questionsToBeDisabled, out questionsToBeEnabled);

            List<Guid> idsOfGroupsToBePropagated = questionnaire.GetGroupsPropagatedByQuestion(questionId).ToList();
            int propagationCount = idsOfGroupsToBePropagated.Any() ? ToPropagationCount(answer) : 0;


            this.ApplyEvent(new NumericQuestionAnswered(userId, questionId, propagationVector, answerTime, answer));

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.PropagationVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.PropagationVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.PropagationVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.PropagationVector)));
            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.PropagationVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.PropagationVector)));

            idsOfGroupsToBePropagated.ForEach(groupId => this.ApplyEvent(new GroupPropagated(groupId, propagationVector, propagationCount)));
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, DateTime answer)
        {
            var answeredQuestion = new Identity(questionId, propagationVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            this.ThrowIfPropagationVectorIsIncorrect(questionnaire, questionId, propagationVector);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.DateTime);
            this.ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, answeredQuestion);


            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? answer : this.GetAnswerOrNull(question);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out answersDeclaredValid, out answersDeclaredInvalid);

            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out questionsToBeDisabled, out questionsToBeEnabled);


            this.ApplyEvent(new DateTimeQuestionAnswered(userId, questionId, propagationVector, answerTime, answer));

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.PropagationVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.PropagationVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.PropagationVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.PropagationVector)));
            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.PropagationVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.PropagationVector)));
        }

        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, decimal selectedValue)
        {
            var answeredQuestion = new Identity(questionId, propagationVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            this.ThrowIfPropagationVectorIsIncorrect(questionnaire, questionId, propagationVector);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.SingleOption);
            ThrowIfValueIsNotOneOfAvailableOptions(questionnaire, questionId, selectedValue);
            this.ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, answeredQuestion);


            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? selectedValue : this.GetAnswerOrNull(question);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out answersDeclaredValid, out answersDeclaredInvalid);

            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out questionsToBeDisabled, out questionsToBeEnabled);


            this.ApplyEvent(new SingleOptionQuestionAnswered(userId, questionId, propagationVector, answerTime, selectedValue));

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.PropagationVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.PropagationVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.PropagationVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.PropagationVector)));
            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.PropagationVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.PropagationVector)));
        }

        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, decimal[] selectedValues)
        {
            var answeredQuestion = new Identity(questionId, propagationVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            this.ThrowIfPropagationVectorIsIncorrect(questionnaire, questionId, propagationVector);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.MultyOption);
            ThrowIfSomeValuesAreNotFromAvailableOptions(questionnaire, questionId, selectedValues);
            this.ThrowIfQuestionOrParentGroupIsDisabled(questionnaire, answeredQuestion);


            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? selectedValues : this.GetAnswerOrNull(question);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out answersDeclaredValid, out answersDeclaredInvalid);

            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out questionsToBeDisabled, out questionsToBeEnabled);


            this.ApplyEvent(new MultipleOptionsQuestionAnswered(userId, questionId, propagationVector, answerTime, selectedValues));

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.PropagationVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.PropagationVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.PropagationVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.PropagationVector)));
            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.PropagationVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.PropagationVector)));
        }

        public void CommentAnswer(Guid userId, Guid questionId, int[] propagationVector, string comment)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            this.ThrowIfPropagationVectorIsIncorrect(questionnaire, questionId, propagationVector);

            this.ApplyEvent(new AnswerCommented(userId, questionId, propagationVector, comment));
        }

        public void SetFlagToAnswer(Guid userId, Guid questionId, int[] propagationVector)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            this.ThrowIfPropagationVectorIsIncorrect(questionnaire, questionId, propagationVector);

            this.ApplyEvent(new FlagSetToAnswer(userId, questionId, propagationVector));
        }

        public void RemoveFlagFromAnswer(Guid userId, Guid questionId, int[] propagationVector)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionnaire, questionId);
            this.ThrowIfPropagationVectorIsIncorrect(questionnaire, questionId, propagationVector);

            this.ApplyEvent(new FlagRemovedFromAnswer(userId, questionId, propagationVector));
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

        private static void ThrowIfQuestionDoesNotExist(IQuestionnaire questionnaire, Guid questionId)
        {
            if (!questionnaire.HasQuestion(questionId))
                throw new InterviewException(string.Format("Question with id '{0}' is not found.", questionId));
        }

        private void ThrowIfPropagationVectorIsIncorrect(IQuestionnaire questionnaire, Guid questionId, int[] propagationVector)
        {
            ThrowIfPropagationVectorIsNull(questionId, propagationVector);

            Guid[] parentPropagatableGroupIdsStartingFromTop = questionnaire.GetParentPropagatableGroupsForQuestionStartingFromTop(questionId).ToArray();

            ThrowIfPropagationVectorLengthDoesNotCorrespondToParentPropagatableGroupsCount(questionId, propagationVector, parentPropagatableGroupIdsStartingFromTop);

            this.ThrowIfSomeOfPropagationVectorValuesAreInvalid(questionId, propagationVector, parentPropagatableGroupIdsStartingFromTop);
        }

        private static void ThrowIfPropagationVectorIsNull(Guid questionId, int[] propagationVector)
        {
            if (propagationVector == null)
                throw new InterviewException(string.Format(
                    "Propagation information for question with id '{0}' is missing. Propagation vector cannot be null.",
                    questionId));
        }

        private static void ThrowIfPropagationVectorLengthDoesNotCorrespondToParentPropagatableGroupsCount(
            Guid questionId, int[] propagationVector, Guid[] parentPropagatableGroups)
        {
            if (propagationVector.Length != parentPropagatableGroups.Length)
                throw new InterviewException(string.Format(
                    "Propagation information for question with id '{0}' is incorrect. " +
                    "Propagation vector has {1} elements, but parent propagatable groups count is {2}.",
                    questionId, propagationVector.Length, parentPropagatableGroups.Length));
        }

        private void ThrowIfSomeOfPropagationVectorValuesAreInvalid(
            Guid questionId, int[] propagationVector, Guid[] parentPropagatableGroupIdsStartingFromTop)
        {
            for (int indexOfPropagationVectorElement = 0; indexOfPropagationVectorElement < propagationVector.Length; indexOfPropagationVectorElement++)
            {
                int propagatableGroupInstanceIndex = propagationVector[indexOfPropagationVectorElement];
                Guid propagatableGroupId = parentPropagatableGroupIdsStartingFromTop[indexOfPropagationVectorElement];

                int propagatableGroupOuterScopePropagationLevel = indexOfPropagationVectorElement;
                int[] propagatableGroupOuterScopePropagationVector = ShrinkPropagationVector(propagationVector, propagatableGroupOuterScopePropagationLevel);
                int countOfPropagatableGroupInstances = this.GetCountOfPropagatableGroupInstances(
                    propagatableGroupId: propagatableGroupId,
                    outerScopePropagationVector: propagatableGroupOuterScopePropagationVector);

                if (propagatableGroupInstanceIndex < 0)
                    throw new InterviewException(string.Format(
                        "Propagation information for question with id '{0}' is incorrect. " +
                        "Propagation vector element with index [{1}] is negative.",
                        questionId, indexOfPropagationVectorElement));

                if (propagatableGroupInstanceIndex >= countOfPropagatableGroupInstances)
                    throw new InterviewException(string.Format(
                        "Propagation information for question with id '{0}' is incorrect. " +
                        "Propagation vector element with index [{1}] refers to instance of propagatable group '{2}' by index [{3}]" +
                        "but propagatable group has only {4} propagated instances.",
                        questionId, indexOfPropagationVectorElement, propagatableGroupId, propagatableGroupInstanceIndex, countOfPropagatableGroupInstances));
            }
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

        private static void ThrowIfSomePropagatingQuestionsReferToNotExistingOrNotPropagatableGroups(IQuestionnaire questionnaire, Guid questionnaireId)
        {
            IEnumerable<Guid> invalidQuestions = questionnaire.GetPropagatingQuestionsWhichReferToMissingOrNotPropagatableGroups();

            if (invalidQuestions.Any())
                throw new InterviewException(string.Format(
                    "Cannot create interview from questionnaire '{1}' because following questions in it are propagating and reference not existing or not propagatable groups:{0}{2}",
                    Environment.NewLine, questionnaireId,
                    string.Join(
                        Environment.NewLine,
                        invalidQuestions.Select(questionId
                            => string.Format("{0}", questionId)))));
        }

        private void ThrowIfQuestionOrParentGroupIsDisabled(IQuestionnaire questionnaire, Identity question)
        {
            if (this.IsQuestionDisabled(question))
                throw new InterviewException(string.Format(
                    "Question {1} is disabled by it's following enablement condition:{0}{2}",
                    Environment.NewLine, 
                    FormatQuestionForException(question, questionnaire),
                    questionnaire.GetCustomEnablementConditionForQuestion(question.Id)));

            IEnumerable<Guid> parentGroupIds = questionnaire.GetAllParentGroupsForQuestion(question.Id);
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperPropagationLevelOrThrow(parentGroupIds, question.PropagationVector, questionnaire);

            foreach (Identity parentGroup in parentGroups)
            {
                if (this.IsGroupDisabled(parentGroup))
                    throw new InterviewException(string.Format(
                        "Question {1} is disabled because parent group {2} is disabled by it's following enablement condition:{0}{3}",
                        Environment.NewLine,
                        FormatQuestionForException(question, questionnaire),
                        FormatGroupForException(parentGroup, questionnaire),
                        questionnaire.GetCustomEnablementConditionForGroup(parentGroup.Id)));
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


        private void PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
            Identity answeredQuestion, IQuestionnaire questionnaire, Func<Identity, object> getAnswer,
            out List<Identity> questionsDeclaredValid, out List<Identity> questionsDeclaredInvalid)
        {
            questionsDeclaredValid = new List<Identity>();
            questionsDeclaredInvalid = new List<Identity>();

            bool? answeredQuestionValidationResult = this.PerformCustomValidationOfQuestion(answeredQuestion, questionnaire, getAnswer);
            switch (answeredQuestionValidationResult)
            {
                case true: questionsDeclaredValid.Add(answeredQuestion); break;
                case false: questionsDeclaredInvalid.Add(answeredQuestion); break;
            }

            List<Identity> dependentQuestionsDeclaredValid;
            List<Identity> dependentQuestionsDeclaredInvalid;
            this.PerformCustomValidationOfDependentQuestions(answeredQuestion, questionnaire, getAnswer,
                out dependentQuestionsDeclaredValid, out dependentQuestionsDeclaredInvalid);

            questionsDeclaredValid.AddRange(dependentQuestionsDeclaredValid);
            questionsDeclaredInvalid.AddRange(dependentQuestionsDeclaredInvalid);
        }

        private void PerformCustomValidationOfDependentQuestions(
            Identity question, IQuestionnaire questionnaire, Func<Identity, object> getAnswer,
            out List<Identity> questionsDeclaredValid, out List<Identity> questionsDeclaredInvalid)
        {
            questionsDeclaredValid = new List<Identity>();
            questionsDeclaredInvalid = new List<Identity>();

            IEnumerable<Guid> dependentQuestionIds = questionnaire.GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(question.Id);
            IEnumerable<Identity> dependentQuestions = this.GetInstancesOfQuestionsWithSameAndDeeperPropagationLevelOrThrow(dependentQuestionIds, question.PropagationVector, questionnaire);

            foreach (Identity dependentQuestion in dependentQuestions)
            {
                bool? dependentQuestionValidationResult = this.PerformCustomValidationOfQuestion(dependentQuestion, questionnaire, getAnswer);
                switch (dependentQuestionValidationResult)
                {
                    case true: questionsDeclaredValid.Add(dependentQuestion); break;
                    case false: questionsDeclaredInvalid.Add(dependentQuestion); break;
                }
            }
        }

        private bool? PerformCustomValidationOfQuestion(Identity question, IQuestionnaire questionnaire, Func<Identity, object> getAnswer)
        {
            if (!questionnaire.IsCustomValidationDefined(question.Id))
                return true;

            string validationExpression = questionnaire.GetCustomValidationExpression(question.Id);

            IEnumerable<Guid> involvedQuestionIds = questionnaire.GetQuestionsInvolvedInCustomValidation(question.Id);
            IEnumerable<Identity> involvedQuestions = GetInstancesOfQuestionsWithSameAndUpperPropagationLevelOrThrow(involvedQuestionIds, question.PropagationVector, questionnaire);

            return this.EvaluateBooleanExpressionIfEnoughAnswers(validationExpression, involvedQuestions, getAnswer, question.Id);
        }


        private void DetermineCustomEnablementStateOfDependentGroups(
            Identity question, IQuestionnaire questionnaire, Func<Identity, object> getAnswer,
            out List<Identity> groupsToBeDisabled, out List<Identity> groupsToBeEnabled)
        {
            groupsToBeDisabled = new List<Identity>();
            groupsToBeEnabled = new List<Identity>();

            IEnumerable<Guid> dependentGroupIds = questionnaire.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(question.Id);
            IEnumerable<Identity> dependentGroups = this.GetInstancesOfGroupsWithSameAndDeeperPropagationLevelOrThrow(dependentGroupIds, question.PropagationVector, questionnaire);

            foreach (Identity dependentGroup in dependentGroups)
            {
                bool? enablementState = this.DetermineCustomEnablementStateOfGroup(dependentGroup, questionnaire, getAnswer);

                bool shouldGroupBeDisabled = enablementState == false;
                bool shouldGroupBeEnabled = enablementState == true;

                if (shouldGroupBeDisabled && !this.IsGroupDisabled(dependentGroup))
                {
                    groupsToBeDisabled.Add(dependentGroup);
                }

                if (shouldGroupBeEnabled && this.IsGroupDisabled(dependentGroup))
                {
                    groupsToBeEnabled.Add(dependentGroup);
                }
            }
        }

        private void DetermineCustomEnablementStateOfDependentQuestions(
            Identity question, IQuestionnaire questionnaire, Func<Identity, object> getAnswer,
            out List<Identity> questionsToBeDisabled, out List<Identity> questionsToBeEnabled)
        {
            questionsToBeDisabled = new List<Identity>();
            questionsToBeEnabled = new List<Identity>();

            IEnumerable<Guid> dependentQuestionIds = questionnaire.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(question.Id);
            IEnumerable<Identity> dependentQuestions = this.GetInstancesOfQuestionsWithSameAndDeeperPropagationLevelOrThrow(dependentQuestionIds, question.PropagationVector, questionnaire);

            foreach (Identity dependentQuestion in dependentQuestions)
            {
                bool? enablementState = this.DetermineCustomEnablementStateOfQuestion(dependentQuestion, questionnaire, getAnswer);

                bool shouldQuestionBeDisabled = enablementState == false;
                bool shouldQuestionBeEnabled = enablementState == true;

                if (shouldQuestionBeDisabled && !this.IsQuestionDisabled(dependentQuestion))
                {
                    questionsToBeDisabled.Add(dependentQuestion);
                }

                if (shouldQuestionBeEnabled && this.IsQuestionDisabled(dependentQuestion))
                {
                    questionsToBeEnabled.Add(dependentQuestion);
                }
            }
        }

        private bool? DetermineCustomEnablementStateOfGroup(Identity group, IQuestionnaire questionnaire, Func<Identity, object> getAnswer)
        {
            IEnumerable<Guid> involvedQuestionIds = questionnaire.GetQuestionsInvolvedInCustomEnablementConditionForGroup(@group.Id);
            IEnumerable<Identity> involvedQuestions = GetInstancesOfQuestionsWithSameAndUpperPropagationLevelOrThrow(involvedQuestionIds, @group.PropagationVector, questionnaire);

            string enablementCondition = questionnaire.GetCustomEnablementConditionForGroup(group.Id);

            return this.EvaluateBooleanExpressionIfEnoughAnswers(enablementCondition, involvedQuestions, getAnswer);
        }

        private bool? DetermineCustomEnablementStateOfQuestion(Identity question, IQuestionnaire questionnaire, Func<Identity, object> getAnswer)
        {
            IEnumerable<Guid> involvedQuestionIds = questionnaire.GetQuestionsInvolvedInCustomEnablementConditionForQuestion(question.Id);
            IEnumerable<Identity> involvedQuestions = GetInstancesOfQuestionsWithSameAndUpperPropagationLevelOrThrow(involvedQuestionIds, question.PropagationVector, questionnaire);

            string enablementCondition = questionnaire.GetCustomEnablementConditionForQuestion(question.Id);

            return this.EvaluateBooleanExpressionIfEnoughAnswers(enablementCondition, involvedQuestions, getAnswer);
        }


        private static IEnumerable<Identity> GetInstancesOfQuestionsWithSameAndUpperPropagationLevelOrThrow(
            IEnumerable<Guid> questionIds, int[] propagationVector, IQuestionnaire questionnare)
        {
            int vectorPropagationLevel = propagationVector.Length;

            foreach (Guid questionId in questionIds)
            {
                int questionPropagationLevel = questionnare.GetPropagationLevelForQuestion(questionId);

                if (questionPropagationLevel > vectorPropagationLevel)
                    throw new InterviewException(string.Format(
                        "Question {0} expected to have propagation level not deeper than {1} but it is {2}.",
                        FormatQuestionForException(questionId, questionnare), vectorPropagationLevel, questionPropagationLevel));

                int[] questionPropagationVector = ShrinkPropagationVector(propagationVector, questionPropagationLevel);

                yield return new Identity(questionId, questionPropagationVector);
            }
        }

        private IEnumerable<Identity> GetInstancesOfQuestionsWithSameAndDeeperPropagationLevelOrThrow(
            IEnumerable<Guid> questionIds, int[] propagationVector, IQuestionnaire questionnare)
        {
            int vectorPropagationLevel = propagationVector.Length;

            foreach (Guid questionId in questionIds)
            {
                int questionPropagationLevel = questionnare.GetPropagationLevelForQuestion(questionId);

                if (questionPropagationLevel < vectorPropagationLevel)
                    throw new InterviewException(string.Format(
                        "Question {0} expected to have propagation level not upper than {1} but it is {2}.",
                        FormatQuestionForException(questionId, questionnare), vectorPropagationLevel, questionPropagationLevel));

                Guid[] parentPropagatableGroupsStartingFromTop = questionnare.GetParentPropagatableGroupsForQuestionStartingFromTop(questionId).ToArray();
                IEnumerable<int[]> questionPropagationVectors = this.ExtendPropagationVector(propagationVector, questionPropagationLevel, parentPropagatableGroupsStartingFromTop);

                foreach (int[] questionPropagationVector in questionPropagationVectors)
                {
                    yield return new Identity(questionId, questionPropagationVector);
                }
            }
        }

        private static IEnumerable<Identity> GetInstancesOfGroupsWithSameAndUpperPropagationLevelOrThrow(
            IEnumerable<Guid> groupIds, int[] propagationVector, IQuestionnaire questionnare)
        {
            int vectorPropagationLevel = propagationVector.Length;

            foreach (Guid groupId in groupIds)
            {
                int groupPropagationLevel = questionnare.GetPropagationLevelForGroup(groupId);

                if (groupPropagationLevel > vectorPropagationLevel)
                    throw new InterviewException(string.Format(
                        "Group {0} expected to have propagation level not deeper than {1} but it is {2}.",
                        FormatGroupForException(groupId, questionnare), vectorPropagationLevel, groupPropagationLevel));

                int[] groupPropagationVector = ShrinkPropagationVector(propagationVector, groupPropagationLevel);

                yield return new Identity(groupId, groupPropagationVector);
            }
        }

        private IEnumerable<Identity> GetInstancesOfGroupsWithSameAndDeeperPropagationLevelOrThrow(
            IEnumerable<Guid> groupIds, int[] propagationVector, IQuestionnaire questionnare)
        {
            int vectorPropagationLevel = propagationVector.Length;

            foreach (Guid groupId in groupIds)
            {
                int groupPropagationLevel = questionnare.GetPropagationLevelForGroup(groupId);

                if (groupPropagationLevel < vectorPropagationLevel)
                    throw new InterviewException(string.Format(
                        "Group {0} expected to have propagation level not upper than {1} but it is {2}.",
                        FormatGroupForException(groupId, questionnare), vectorPropagationLevel, groupPropagationLevel));

                Guid[] parentPropagatableGroupsStartingFromTop = questionnare.GetParentPropagatableGroupsForGroupStartingFromTop(groupId).ToArray();
                IEnumerable<int[]> groupPropagationVectors = this.ExtendPropagationVector(propagationVector, groupPropagationLevel, parentPropagatableGroupsStartingFromTop);

                foreach (int[] groupPropagationVector in groupPropagationVectors)
                {
                    yield return new Identity(groupId, groupPropagationVector);
                }
            }
        }


        private bool? EvaluateBooleanExpressionIfEnoughAnswers(string expression, IEnumerable<Identity> involvedQuestions,
            Func<Identity, object> getAnswer, Guid? thisIdentifierQuestionId = null)
        {
            Dictionary<Guid, object> involvedAnswers = involvedQuestions.ToDictionary(
                involvedQuestion => involvedQuestion.Id,
                involvedQuestion => getAnswer(involvedQuestion));

            bool someOfInvolvedQuestionsAreNotAnswered = involvedAnswers.Values.Any(answer => answer == null);
            if (someOfInvolvedQuestionsAreNotAnswered)
                return null;

            bool isSpecialThisIdentifierSupportedByExpression = thisIdentifierQuestionId.HasValue;

            var mapIdentifierToQuestionId = isSpecialThisIdentifierSupportedByExpression
                ? (Func<string, Guid>) (identifier => GetQuestionIdByExpressionIdentifierIncludingThis(identifier, thisIdentifierQuestionId.Value))
                : (Func<string, Guid>) (identifier => GetQuestionIdByExpressionIdentifierExcludingThis(identifier));

            return this.ExpressionProcessor.EvaluateBooleanExpression(expression,
                getValueForIdentifier: idetifier => involvedAnswers[mapIdentifierToQuestionId(idetifier)]);
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

        private int GetCountOfPropagatableGroupInstances(Guid propagatableGroupId, int[] outerScopePropagationVector)
        {
            string propagatableGroupKey = ConvertIdAndPropagationVectorToString(propagatableGroupId, outerScopePropagationVector);

            return this.propagatedGroupInstanceCounts.ContainsKey(propagatableGroupKey)
                ? this.propagatedGroupInstanceCounts[propagatableGroupKey]
                : 0;
        }

        private bool IsGroupDisabled(Identity group)
        {
            string groupKey = ConvertIdAndPropagationVectorToString(group.Id, group.PropagationVector);

            return this.disabledGroups.Contains(groupKey);
        }

        private bool IsQuestionDisabled(Identity question)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(question.Id, question.PropagationVector);

            return this.disabledQuestions.Contains(questionKey);
        }

        private object GetAnswerOrNull(Identity question)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(question.Id, question.PropagationVector);

            return this.answers.ContainsKey(questionKey)
                ? this.answers[questionKey]
                : null;
        }

        private static readonly int[] EmptyPropagationVector = {};

        private static int[] ShrinkPropagationVector(int[] propagationVector, int length)
        {
            if (length == 0)
                return EmptyPropagationVector;

            if (length == propagationVector.Length)
                return propagationVector;

            if (length > propagationVector.Length)
                throw new ArgumentException(string.Format("Cannot shrink vector with length {0} to bigger length {1}.", propagationVector.Length, length));

            return propagationVector.Take(length).ToArray();
        }

        /// <remarks>
        /// If propagation vector should be extended, result will be a set of vectors depending on propagation count of correspondifg groups.
        /// </remarks>
        private IEnumerable<int[]> ExtendPropagationVector(int[] propagationVector, int length, Guid[] parentPropagatableGroupsStartingFromTop)
        {
            if (length < propagationVector.Length)
                throw new ArgumentException(string.Format("Cannot extend vector with length {0} to smaller length {1}.", propagationVector.Length, length));

            if (length == propagationVector.Length)
            {
                yield return propagationVector;
                yield break;
            }

            if (length == propagationVector.Length + 1)
            {
                int countOfInstances = this.GetCountOfPropagatableGroupInstances(
                    propagatableGroupId: parentPropagatableGroupsStartingFromTop.Last(),
                    outerScopePropagationVector: propagationVector);

                for (int instanceIndex = 0; instanceIndex < countOfInstances; instanceIndex++)
                {
                    yield return ExtendPropagationVectorWithOneValue(propagationVector, instanceIndex);
                }
            }

            throw new NotImplementedException("This method doed not support propagated groups inside propagated groups, but may easily support it when needed.");
        }

        private static int[] ExtendPropagationVectorWithOneValue(int[] propagationVector, int value)
        {
            return new List<int>(propagationVector) { value }.ToArray();
        }

        private static bool AreEqual(Identity identityA, Identity identityB)
        {
            return identityA.Id == identityB.Id
                && Enumerable.SequenceEqual(identityA.PropagationVector, identityB.PropagationVector);
        }

        private static int ToPropagationCount(decimal decimalValue)
        {
            return (int) decimalValue;
        }

        private static string JoinDecimalsWithComma(IEnumerable<decimal> values)
        {
            return string.Join(", ", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));
        }

        private static string FormatQuestionForException(Identity question, IQuestionnaire questionnaire)
        {
            return string.Format("'{0} ({1})'", question.Id, string.Join("-", question.PropagationVector));
        }

        private static string FormatQuestionForException(Guid questionId, IQuestionnaire questionnaire)
        {
            return string.Format("'{0}'", questionId);
        }

        private static string FormatGroupForException(Identity group, IQuestionnaire questionnaire)
        {
            return string.Format("'{0} ({1})'", group.Id, string.Join("-", group.PropagationVector));
        }

        private static string FormatGroupForException(Guid groupId, IQuestionnaire questionnaire)
        {
            return string.Format("'{0}'", groupId);
        }

        /// <remarks>
        /// The opposite operation (get id or vector from string) should never be performed!
        /// This is one-way transformation. Opposite operation is too slow.
        /// If you need to compactify data and get it back, you should use another datatype, not a string.
        /// </remarks>
        private static string ConvertIdAndPropagationVectorToString(Guid id, int[] propagationVector)
        {
            return string.Format("{0:N}:{1}", id, string.Join("-", propagationVector));
        }
    }
}
