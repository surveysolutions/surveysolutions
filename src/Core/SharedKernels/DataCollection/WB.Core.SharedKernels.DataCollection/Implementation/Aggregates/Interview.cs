using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire.Completed;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using InterviewDeleted = WB.Core.SharedKernels.DataCollection.Events.Interview.InterviewDeleted;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Interview : AggregateRootMappedByConvention, ISnapshotable<InterviewState>
    {
        #region Constants

        private static readonly int[] EmptyPropagationVector = {};

        #endregion

        #region State

        private Guid questionnaireId;
        private long questionnaireVersion;
        private bool wasCompleted;
        private InterviewStatus status;
        private Dictionary<string, object> answersSupportedInExpressions = new Dictionary<string, object>();
        private HashSet<string> answeredQuestions = new HashSet<string>();
        private HashSet<string> disabledGroups = new HashSet<string>();
        private HashSet<string> disabledQuestions = new HashSet<string>();
        private Dictionary<string, int> propagatedGroupInstanceCounts = new Dictionary<string, int>();
        private HashSet<string> validAnsweredQuestions = new HashSet<string>();
        private HashSet<string> invalidAnsweredQuestions = new HashSet<string>();

        private void Apply(InterviewCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        private void Apply(InterviewSynchronized @event)
        {
            this.questionnaireId = @event.InterviewData.QuestionnaireId;
            this.questionnaireVersion = @event.InterviewData.QuestionnaireVersion;
            this.status = @event.InterviewData.Status;
            this.wasCompleted = @event.InterviewData.WasCompleted;

            this.answersSupportedInExpressions = @event.InterviewData.Answers == null
                ? new Dictionary<string, object>()
                : @event.InterviewData
                    .Answers
                    .Where(question => !(question.Answer is GeoPosition))
                    .ToDictionary(
                        question => ConvertIdAndPropagationVectorToString(question.Id, question.PropagationVector),
                        question => question.Answer);

            this.answeredQuestions = new HashSet<string>(
                @event.InterviewData.Answers.Select(question => ConvertIdAndPropagationVectorToString(question.Id, question.PropagationVector)));

            this.disabledGroups = ToHashSetOfIdAndPropagationVectorStrings(@event.InterviewData.DisabledGroups);
            this.disabledQuestions = ToHashSetOfIdAndPropagationVectorStrings(@event.InterviewData.DisabledQuestions);

            this.propagatedGroupInstanceCounts = @event.InterviewData.PropagatedGroupInstanceCounts.ToDictionary(
                pair => ConvertIdAndPropagationVectorToString(pair.Key.Id, pair.Key.PropagationVector),
                pair => pair.Value);

            this.validAnsweredQuestions = ToHashSetOfIdAndPropagationVectorStrings(@event.InterviewData.ValidAnsweredQuestions);
            this.invalidAnsweredQuestions = ToHashSetOfIdAndPropagationVectorStrings(@event.InterviewData.InvalidAnsweredQuestions);
        }

        private void Apply(SynchronizationMetadataApplied @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.status = @event.Status;
        }

        private void Apply(TextQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.Answer;
            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(NumericQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.Answer;
            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(DateTimeQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.Answer;
            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(SingleOptionQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.SelectedValue;
            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(MultipleOptionsQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.SelectedValues;
            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(GeoLocationQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(AnswerDeclaredValid @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.validAnsweredQuestions.Add(questionKey);
            this.invalidAnsweredQuestions.Remove(questionKey);
        }

        private void Apply(AnswerDeclaredInvalid @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.validAnsweredQuestions.Remove(questionKey);
            this.invalidAnsweredQuestions.Add(questionKey);
        }

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

        private void Apply(InterviewStatusChanged @event)
        {
            this.status = @event.Status;
        }

        private void Apply(SupervisorAssigned @event) {}

        private void Apply(InterviewerAssigned @event) {}

        private void Apply(InterviewDeleted @event) {}

        private void Apply(InterviewRestored @event) {}

        private void Apply(InterviewCompleted @event)
        {
            this.wasCompleted = true;
        }

        private void Apply(InterviewRestarted @event) {}

        private void Apply(InterviewApproved @event) {}

        private void Apply(InterviewRejected @event)
        {
            this.wasCompleted = false;
        }

        private void Apply(InterviewDeclaredValid @event) {}

        private void Apply(InterviewDeclaredInvalid @event) {}

        private void Apply(AnswerRemoved @event)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions.Remove(questionKey);
            this.answeredQuestions.Remove(questionKey);
            this.disabledQuestions.Remove(questionKey);
            this.validAnsweredQuestions.Remove(questionKey);
            this.invalidAnsweredQuestions.Remove(questionKey);
        }

        public InterviewState CreateSnapshot()
        {
            return new InterviewState(questionnaireId, questionnaireVersion, status, answersSupportedInExpressions, answeredQuestions, disabledGroups,
                                      disabledQuestions, propagatedGroupInstanceCounts, validAnsweredQuestions, invalidAnsweredQuestions, this.wasCompleted);
        }

        public void RestoreFromSnapshot(InterviewState snapshot)
        {
            questionnaireId = snapshot.QuestionnaireId;
            questionnaireVersion = snapshot.QuestionnaireVersion;
            status = snapshot.Status;
            answersSupportedInExpressions = snapshot.AnswersSupportedInExpressions;
            answeredQuestions = snapshot.AnsweredQuestions;
            disabledGroups = snapshot.DisabledGroups;
            disabledQuestions = snapshot.DisabledQuestions;
            propagatedGroupInstanceCounts = snapshot.PropagatedGroupInstanceCounts;
            validAnsweredQuestions = snapshot.ValidAnsweredQuestions;
            invalidAnsweredQuestions = snapshot.InvalidAnsweredQuestions;
            this.wasCompleted = snapshot.WasCompleted;
        }

        #endregion

        #region Dependencies

        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

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

        public Interview(Guid id, Guid userId, Guid questionnaireId, Dictionary<Guid, object> answersToFeaturedQuestions,
                         DateTime answersTime, Guid supervisorId)
            : base(id)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireId);
            ThrowIfSomeQuestionsHaveInvalidCustomValidationExpressions(questionnaire, questionnaireId);
            ThrowIfSomeGroupsHaveInvalidCustomEnablementConditions(questionnaire, questionnaireId);
            ThrowIfSomeQuestionsHaveInvalidCustomEnablementConditions(questionnaire, questionnaireId);
            ThrowIfSomePropagatingQuestionsReferToNotExistingOrNotPropagatableGroups(questionnaire, questionnaireId);


            List<Identity> initiallyDisabledGroups = GetGroupsToBeDisabledInJustCreatedInterview(questionnaire);
            List<Identity> initiallyDisabledQuestions = GetQuestionsToBeDisabledInJustCreatedInterview(questionnaire);


            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            initiallyDisabledGroups.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.PropagationVector)));
            initiallyDisabledQuestions.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.PropagationVector)));


#warning TLK: this implementation is incorrect, I cannot use other methods here as is because there might be exceptions and events are raised
            foreach (KeyValuePair<Guid, object> answerToFeaturedQuestion in answersToFeaturedQuestions)
            {
                Guid questionId = answerToFeaturedQuestion.Key;
                object answer = answerToFeaturedQuestion.Value;

                ThrowIfQuestionDoesNotExist(questionId, questionnaire);

                QuestionType questionType = questionnaire.GetQuestionType(questionId);

                switch (questionType)
                {
                    case QuestionType.Text:
                        this.AnswerTextQuestion(userId, questionId, EmptyPropagationVector, answersTime, (string)answer);
                        break;

                    case QuestionType.AutoPropagate:
                    case QuestionType.Numeric:
                        this.AnswerNumericQuestion(userId, questionId, EmptyPropagationVector, answersTime, (decimal)answer);
                        break;

                    case QuestionType.DateTime:
                        this.AnswerDateTimeQuestion(userId, questionId, EmptyPropagationVector, answersTime, (DateTime)answer);
                        break;

                    case QuestionType.SingleOption:
                        this.AnswerSingleOptionQuestion(userId, questionId, EmptyPropagationVector, answersTime, (decimal)answer);
                        break;

                    case QuestionType.MultyOption:
                        this.AnswerMultipleOptionsQuestion(userId, questionId, EmptyPropagationVector, answersTime, (decimal[])answer);
                        break;

                    case QuestionType.GpsCoordinates:
                    default:
                        throw new InterviewException(string.Format(
                            "Question {0} has type {1} which is not supported as initial pre-filled question.",
                            questionId, questionType));
                }
            }
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Created, InterviewStatus.SupervisorAssigned);

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, InterviewStatus interviewStatus, AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta):base(id)
        {
            this.ApplySynchronizationMetadata(id, userId, questionnaireId, interviewStatus, featuredQuestionsMeta);
        }

        public void SynchronizeInterview(Guid userId, InterviewSynchronizationDto synchronizedInterview)
        {
            this.ApplyEvent(new InterviewSynchronized(synchronizedInterview));
        }

        public void ApplySynchronizationMetadata(Guid id, Guid userId, Guid questionnaireId, InterviewStatus interviewStatus, AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta)
        {
            if (this.status == InterviewStatus.Deleted)
                Restore(userId);
            else
                ThrowIfStatusNotAllowedToBeChangedWithMetadata(interviewStatus);

            ApplyEvent(new SynchronizationMetadataApplied(userId, questionnaireId, 
                                                          interviewStatus,
                                                          featuredQuestionsMeta));
            ApplyEvent(new InterviewStatusChanged(interviewStatus, comment: null));
        }

        public void AnswerTextQuestion(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, string answer)
        {
            var answeredQuestion = new Identity(questionId, propagationVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfPropagationVectorIsIncorrect(questionId, propagationVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Text);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);


            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? answer : this.GetAnswerSupportedInExpressionsOrNull(question);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out answersDeclaredValid, out answersDeclaredInvalid);

            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, this.GetCountOfPropagatableGroupInstances, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, this.GetCountOfPropagatableGroupInstances, out questionsToBeDisabled, out questionsToBeEnabled);


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
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfPropagationVectorIsIncorrect(questionId, propagationVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.AutoPropagate, QuestionType.Numeric);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);

            if (questionnaire.ShouldQuestionPropagateGroups(questionId))
            {
                ThrowIfAnswerCannotBeUsedAsPropagationCount(questionId, answer, questionnaire);
            }


            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? answer : this.GetAnswerSupportedInExpressionsOrNull(question);

            List<Guid> idsOfGroupsToBePropagated = questionnaire.GetGroupsPropagatedByQuestion(questionId).ToList();
            int propagationCount = idsOfGroupsToBePropagated.Any() ? ToPropagationCount(answer) : 0;

            Func<Guid, int[], bool> isGroupBeingPropagated = (groupId, groupOuterScopePropagationVector)
                => idsOfGroupsToBePropagated.Contains(groupId)
                && Enumerable.SequenceEqual(groupOuterScopePropagationVector, propagationVector);

            Func<Guid, int[], int> getCountOfPropagatableGroupInstances = (groupId, groupOuterPropagationVector)
                => isGroupBeingPropagated(groupId, groupOuterPropagationVector)
                    ? propagationCount
                    : this.GetCountOfPropagatableGroupInstances(groupId, groupOuterPropagationVector);

            List<Identity> answersToRemove = this.GetAnswersToRemoveIfPropagationCountIsBeingDecreased(
                idsOfGroupsToBePropagated, propagationCount, propagationVector, questionnaire);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out answersDeclaredValid, out answersDeclaredInvalid);

            List<Identity> initializedGroupsToBeDisabled, initializedGroupsToBeEnabled, initializedQuestionsToBeDisabled, initializedQuestionsToBeEnabled;
            this.DetermineCustomEnablementStateOfGroupsInitializedByIncreasedPropagation(
                idsOfGroupsToBePropagated, propagationCount, propagationVector, questionnaire, getAnswer, getCountOfPropagatableGroupInstances,
                out initializedGroupsToBeDisabled, out initializedGroupsToBeEnabled);
            this.DetermineCustomEnablementStateOfQuestionsInitializedByIncreasedPropagation(
                idsOfGroupsToBePropagated, propagationCount, propagationVector, questionnaire, getAnswer, getCountOfPropagatableGroupInstances,
                out initializedQuestionsToBeDisabled, out initializedQuestionsToBeEnabled);

            List<Identity> dependentGroupsToBeDisabled, dependentGroupsToBeEnabled, dependentQuestionsToBeDisabled, dependentQuestionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, getCountOfPropagatableGroupInstances,
                out dependentGroupsToBeDisabled, out dependentGroupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, getCountOfPropagatableGroupInstances,
                out dependentQuestionsToBeDisabled, out dependentQuestionsToBeEnabled);


            this.ApplyEvent(new NumericQuestionAnswered(userId, questionId, propagationVector, answerTime, answer));

            idsOfGroupsToBePropagated.ForEach(groupId => this.ApplyEvent(new GroupPropagated(groupId, propagationVector, propagationCount)));

            answersToRemove.ForEach(question => this.ApplyEvent(new AnswerRemoved(question.Id, question.PropagationVector)));

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.PropagationVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.PropagationVector)));

            initializedGroupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.PropagationVector)));
            initializedGroupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.PropagationVector)));
            initializedQuestionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.PropagationVector)));
            initializedQuestionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.PropagationVector)));

            dependentGroupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.PropagationVector)));
            dependentGroupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.PropagationVector)));
            dependentQuestionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.PropagationVector)));
            dependentQuestionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.PropagationVector)));
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, DateTime answer)
        {
            var answeredQuestion = new Identity(questionId, propagationVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfPropagationVectorIsIncorrect(questionId, propagationVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.DateTime);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);


            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? answer : this.GetAnswerSupportedInExpressionsOrNull(question);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out answersDeclaredValid, out answersDeclaredInvalid);

            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, this.GetCountOfPropagatableGroupInstances, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, this.GetCountOfPropagatableGroupInstances, out questionsToBeDisabled, out questionsToBeEnabled);


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
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfPropagationVectorIsIncorrect(questionId, propagationVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.SingleOption);
            ThrowIfValueIsNotOneOfAvailableOptions(questionId, selectedValue, questionnaire);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);


            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? selectedValue : this.GetAnswerSupportedInExpressionsOrNull(question);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out answersDeclaredValid, out answersDeclaredInvalid);

            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, this.GetCountOfPropagatableGroupInstances, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, this.GetCountOfPropagatableGroupInstances, out questionsToBeDisabled, out questionsToBeEnabled);


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
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfPropagationVectorIsIncorrect(questionId, propagationVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.MultyOption);
            ThrowIfSomeValuesAreNotFromAvailableOptions(questionId, selectedValues, questionnaire);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);


            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? selectedValues : this.GetAnswerSupportedInExpressionsOrNull(question);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, out answersDeclaredValid, out answersDeclaredInvalid);

            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, this.GetCountOfPropagatableGroupInstances, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, questionnaire, getAnswer, this.GetCountOfPropagatableGroupInstances, out questionsToBeDisabled, out questionsToBeEnabled);


            this.ApplyEvent(new MultipleOptionsQuestionAnswered(userId, questionId, propagationVector, answerTime, selectedValues));

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.PropagationVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.PropagationVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.PropagationVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.PropagationVector)));
            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.PropagationVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.PropagationVector)));
        }

        public void AnswerGeoLocationQuestion(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, 
            double latitude, double longitude, double accuracy, DateTimeOffset timestamp)
        {
            var answeredQuestion = new Identity(questionId, propagationVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfPropagationVectorIsIncorrect(questionId, propagationVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.GpsCoordinates);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);

            this.ApplyEvent(new GeoLocationQuestionAnswered(userId, questionId, propagationVector, answerTime, latitude, longitude, accuracy, timestamp));

            this.ApplyEvent(new AnswerDeclaredValid(questionId, propagationVector));
        }

        public void AnswerSingleOptionLinkedQuestion(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, int[] selectedPropagationVector)
        {
            var answeredQuestion = new Identity(questionId, propagationVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfPropagationVectorIsIncorrect(questionId, propagationVector, questionnaire);
            
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.SingleOption);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);

            Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
            var answeredLinkedQuestion = new Identity(linkedQuestionId, selectedPropagationVector);

            this.ThrowIfPropagationVectorIsIncorrect(linkedQuestionId, selectedPropagationVector, questionnaire);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredLinkedQuestion, questionnaire);
            this.ThrowIfLinkedQuestionDoesNotHaveAnswer(answeredQuestion, answeredLinkedQuestion, questionnaire);


            this.ApplyEvent(new SingleOptionLinkedQuestionAnswered(userId, questionId, propagationVector, answerTime, selectedPropagationVector));

            this.ApplyEvent(new AnswerDeclaredValid(questionId, propagationVector));
        }

        public void AnswerMultipleOptionsLinkedQuestion(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, int[][] selectedPropagationVectors)
        {
            var answeredQuestion = new Identity(questionId, propagationVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfPropagationVectorIsIncorrect(questionId, propagationVector, questionnaire);
            this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.MultyOption);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);

            Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
            foreach (var answeredLinkedQuestion in selectedPropagationVectors.Select(selectedPropagationVector => new Identity(linkedQuestionId, selectedPropagationVector)))
            {
                this.ThrowIfPropagationVectorIsIncorrect(linkedQuestionId, answeredLinkedQuestion.PropagationVector, questionnaire);    
                this.ThrowIfQuestionOrParentGroupIsDisabled(answeredLinkedQuestion, questionnaire);
                this.ThrowIfLinkedQuestionDoesNotHaveAnswer(answeredQuestion, answeredLinkedQuestion, questionnaire);
            }
            

            this.ApplyEvent(new MultipleOptionsLinkedQuestionAnswered(userId, questionId, propagationVector, answerTime, selectedPropagationVectors));

            this.ApplyEvent(new AnswerDeclaredValid(questionId, propagationVector));
        }

        public void ReevaluateSynchronizedInterview()
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            List<Identity> questionsToBeEnabled=new List<Identity>();
            List<Identity> questionsToBeDisabled = new List<Identity>();

            List<Identity> groupsToBeEnabled = new List<Identity>();
            List<Identity> groupsToBeDisabled = new List<Identity>();

            List<Identity> questionsDeclaredValid = new List<Identity>();
            List<Identity> questionsDeclaredInvalid = new List<Identity>();

            foreach (var groupWithNotEmptyCustomEnablementCondition in questionnaire.GetAllGroupsWithNotEmptyCustomEnablementConditions())
            {
                var availablePropagationLevels = this.AvailablePropagationLevelsForGroup(questionnaire, groupWithNotEmptyCustomEnablementCondition);

                foreach (var availablePropagationLevel in availablePropagationLevels)
                {
                    Identity groupIdAtInterview = new Identity(groupWithNotEmptyCustomEnablementCondition, availablePropagationLevel);

                    PutToCorrespondingListAccordingToEnablementStateChange(groupIdAtInterview, groupsToBeEnabled, groupsToBeDisabled,
                        isNewStateEnabled: this.ShouldGroupBeEnabledByCustomEnablementCondition(groupIdAtInterview, questionnaire,
                            GetAnswerSupportedInExpressionsOrNull),
                        isOldStateEnabled: !this.IsGroupDisabled(groupIdAtInterview));
                }
            }

            foreach (var questionWithNotEmptyEnablementCondition in questionnaire.GetAllQuestionsWithNotEmptyCustomEnablementConditions())
            {
                var availablePropagationLevels = this.AvailablePropagationLevelsForQuestion(questionnaire, questionWithNotEmptyEnablementCondition);

                foreach (var availablePropagationLevel in availablePropagationLevels)
                {
                    Identity questionIdAtInterview = new Identity(questionWithNotEmptyEnablementCondition, availablePropagationLevel);

                    PutToCorrespondingListAccordingToEnablementStateChange(questionIdAtInterview, questionsToBeEnabled,
                        questionsToBeDisabled,
                        isNewStateEnabled:
                            this.ShouldQuestionBeEnabledByCustomEnablementCondition(questionIdAtInterview, questionnaire,
                                GetAnswerSupportedInExpressionsOrNull),
                        isOldStateEnabled: !this.IsQuestionDisabled(questionIdAtInterview));
                }
            }

            foreach (var questionWithNotEmptyValidationExpression in questionnaire.GetAllQuestionsWithNotEmptyValidationExpressions())
            {
                var availablePropagationLevels = this.AvailablePropagationLevelsForQuestion(questionnaire,
                    questionWithNotEmptyValidationExpression);

                foreach (var availablePropagationLevel in availablePropagationLevels)
                {
                    Identity questionIdAtInterview = new Identity(questionWithNotEmptyValidationExpression, availablePropagationLevel);
                   

                    if(IsQuestionOrParentGroupDisabled(questionIdAtInterview, questionnaire, groupsToBeDisabled.Contains, questionsToBeDisabled.Contains))
                        continue;

                    bool? dependentQuestionValidationResult = this.PerformCustomValidationOfQuestion(questionIdAtInterview, questionnaire,
                        GetAnswerSupportedInExpressionsOrNull);

                    switch (dependentQuestionValidationResult)
                    {
                        case true:
                            questionsDeclaredValid.Add(questionIdAtInterview);
                            break;
                        case false:
                            questionsDeclaredInvalid.Add(questionIdAtInterview);
                            break;
                    }
                }
            }

            questionsDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.PropagationVector)));
            questionsDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.PropagationVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.PropagationVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.PropagationVector)));

            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.PropagationVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.PropagationVector)));
        }

        private IEnumerable<int[]> AvailablePropagationLevelsForGroup(IQuestionnaire questionnaire, Guid groupdId)
        {
            int groupPropagationLevel = questionnaire.GetPropagationLevelForGroup(groupdId);

            Guid[] parentPropagatableGroupsStartingFromTop =
                questionnaire.GetParentPropagatableGroupsAndGroupItselfIfPropagatableStartingFromTop(groupdId)
                    .ToArray();

            var availablePropagationLevels = this.ExtendPropagationVector(EmptyPropagationVector, groupPropagationLevel,
                parentPropagatableGroupsStartingFromTop, this.GetCountOfPropagatableGroupInstances);
            return availablePropagationLevels;
        }

        private IEnumerable<int[]> AvailablePropagationLevelsForQuestion(IQuestionnaire questionnaire, Guid questionId)
        {
            int questionPropagationLevel = questionnaire.GetPropagationLevelForQuestion(questionId);

            Guid[] parentPropagatableGroupsStartingFromTop =
                questionnaire.GetParentPropagatableGroupsForQuestionStartingFromTop(questionId)
                    .ToArray();

            var availablePropagationLevels = this.ExtendPropagationVector(EmptyPropagationVector, questionPropagationLevel,
                parentPropagatableGroupsStartingFromTop, this.GetCountOfPropagatableGroupInstances);

            return availablePropagationLevels;
        }

        public void CommentAnswer(Guid userId, Guid questionId, int[] propagationVector, DateTime commentTime,string comment)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfPropagationVectorIsIncorrect(questionId, propagationVector, questionnaire);

            this.ApplyEvent(new AnswerCommented(userId, questionId, propagationVector, commentTime, comment));
        }

        public void SetFlagToAnswer(Guid userId, Guid questionId, int[] propagationVector)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfPropagationVectorIsIncorrect(questionId, propagationVector, questionnaire);

            this.ApplyEvent(new FlagSetToAnswer(userId, questionId, propagationVector));
        }

        public void RemoveFlagFromAnswer(Guid userId, Guid questionId, int[] propagationVector)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfPropagationVectorIsIncorrect(questionId, propagationVector, questionnaire);

            this.ApplyEvent(new FlagRemovedFromAnswer(userId, questionId, propagationVector));
        }

        public void AssignSupervisor(Guid userId, Guid supervisorId)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Created, InterviewStatus.SupervisorAssigned);

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public void AssignInterviewer(Guid userId, Guid interviewerId)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.SupervisorAssigned, InterviewStatus.InterviewerAssigned, InterviewStatus.RejectedBySupervisor);

            this.ApplyEvent(new InterviewerAssigned(userId, interviewerId));
            if (!this.wasCompleted && this.status != InterviewStatus.InterviewerAssigned)
            {
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
            }
        }

        public void Delete(Guid userId)
        {
            this.ThrowIfInterviewWasCompleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.Created, InterviewStatus.SupervisorAssigned, InterviewStatus.InterviewerAssigned, InterviewStatus.Restored);

            this.ApplyEvent(new InterviewDeleted(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Deleted, comment: null));
        }

        public void Restore(Guid userId)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Deleted);

            this.ApplyEvent(new InterviewRestored(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Restored, comment: null));
        }

        public void Complete(Guid userId, string comment)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.InterviewerAssigned, InterviewStatus.Restarted, InterviewStatus.RejectedBySupervisor);

            bool isInterviewValid = this.HasInvalidAnswers() || this.HasNotAnsweredMandatoryQuestions(questionnaire);

            this.ApplyEvent(new InterviewCompleted(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Completed, comment));

            this.ApplyEvent(isInterviewValid
                ? new InterviewDeclaredValid() as object
                : new InterviewDeclaredInvalid() as object);
        }

        public void Restart(Guid userId, string comment)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed);

            this.ApplyEvent(new InterviewRestarted(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Restarted, comment));
        }

        public void Approve(Guid userId, string comment)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor);

            this.ApplyEvent(new InterviewApproved(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor, comment));
        }

        public void Reject(Guid userId, string comment)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed, InterviewStatus.ApprovedBySupervisor);

            this.ApplyEvent(new InterviewRejected(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.RejectedBySupervisor, comment));
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

        private void ThrowIfInterviewWasCompleted()
        {
            if (this.wasCompleted)
                throw new InterviewException(string.Format("Interview was completed by interviewer and cannot be deleted"));
        }

        private static void ThrowIfQuestionDoesNotExist(Guid questionId, IQuestionnaire questionnaire)
        {
            if (!questionnaire.HasQuestion(questionId))
                throw new InterviewException(string.Format("Question with id '{0}' is not found.", questionId));
        }

        private void ThrowIfPropagationVectorIsIncorrect(Guid questionId, int[] propagationVector, IQuestionnaire questionnaire)
        {
            ThrowIfPropagationVectorIsNull(questionId, propagationVector, questionnaire);

            Guid[] parentPropagatableGroupIdsStartingFromTop = questionnaire.GetParentPropagatableGroupsForQuestionStartingFromTop(questionId).ToArray();

            ThrowIfPropagationVectorLengthDoesNotCorrespondToParentPropagatableGroupsCount(questionId, propagationVector, parentPropagatableGroupIdsStartingFromTop, questionnaire);

            this.ThrowIfSomeOfPropagationVectorValuesAreInvalid(questionId, propagationVector, parentPropagatableGroupIdsStartingFromTop, questionnaire);
        }

        private static void ThrowIfPropagationVectorIsNull(Guid questionId, int[] propagationVector, IQuestionnaire questionnaire)
        {
            if (propagationVector == null)
                throw new InterviewException(string.Format(
                    "Propagation information for question {0} is missing. Propagation vector cannot be null.",
                    FormatQuestionForException(questionId, questionnaire)));
        }

        private static void ThrowIfPropagationVectorLengthDoesNotCorrespondToParentPropagatableGroupsCount(
            Guid questionId, int[] propagationVector, Guid[] parentPropagatableGroups, IQuestionnaire questionnaire)
        {
            if (propagationVector.Length != parentPropagatableGroups.Length)
                throw new InterviewException(string.Format(
                    "Propagation information for question {0} is incorrect. " +
                    "Propagation vector has {1} elements, but parent propagatable groups count is {2}.",
                    FormatQuestionForException(questionId, questionnaire), propagationVector.Length, parentPropagatableGroups.Length));
        }

        private void ThrowIfSomeOfPropagationVectorValuesAreInvalid(
            Guid questionId, int[] propagationVector, Guid[] parentPropagatableGroupIdsStartingFromTop, IQuestionnaire questionnaire)
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
                        "Propagation information for question {0} is incorrect. " +
                        "Propagation vector element with index [{1}] is negative.",
                        FormatQuestionForException(questionId, questionnaire), indexOfPropagationVectorElement));

                if (propagatableGroupInstanceIndex >= countOfPropagatableGroupInstances)
                    throw new InterviewException(string.Format(
                        "Propagation information for question {0} is incorrect. " +
                        "Propagation vector element with index [{1}] refers to instance of propagatable group {2} by index [{3}]" +
                        "but propagatable group has only {4} propagated instances.",
                        FormatQuestionForException(questionId, questionnaire), indexOfPropagationVectorElement,
                        FormatGroupForException(propagatableGroupId, questionnaire), propagatableGroupInstanceIndex, countOfPropagatableGroupInstances));
            }
        }

        private static void ThrowIfQuestionTypeIsNotOneOfExpected(Guid questionId, IQuestionnaire questionnaire, params QuestionType[] expectedQuestionTypes)
        {
            QuestionType questionType = questionnaire.GetQuestionType(questionId);

            bool typeIsNotExpected = !expectedQuestionTypes.Contains(questionType);
            if (typeIsNotExpected)
                throw new InterviewException(string.Format(
                    "Question {0} has type {1}. But one of the following types was expected: {2}.",
                    FormatQuestionForException(questionId, questionnaire), questionType, string.Join(", ", expectedQuestionTypes.Select(type => type.ToString()))));
        }

        private Guid GetLinkedQuestionIdOrThrow(Guid questionId, IQuestionnaire questionnaire)
        {
            Guid? linkedQuestionId = questionnaire.GetQuestionLinkedQuestionId(questionId);
            if(!linkedQuestionId.HasValue)
                throw new InterviewException(string.Format(
                   "Question {0} wasn't linked on any question",
                   FormatQuestionForException(questionId, questionnaire)));

            return linkedQuestionId.Value;
        }

        private void ThrowIfLinkedQuestionDoesNotHaveAnswer(Identity answeredQuestion, Identity answeredLinkedQuestion, IQuestionnaire questionnaire)
        {
            if (!this.WasQuestionAnswered(answeredLinkedQuestion))
            {
                throw new InterviewException(string.Format(
                    "Could not set answer for question {0} because his dependent linked question {1} does not have answer",
                    FormatQuestionForException(answeredQuestion, questionnaire),
                    FormatQuestionForException(answeredLinkedQuestion, questionnaire)));
            }
        }

        private static void ThrowIfValueIsNotOneOfAvailableOptions(Guid questionId, decimal value, IQuestionnaire questionnaire)
        {
            IEnumerable<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId);

            bool valueIsNotOneOfAvailable = !availableValues.Contains(value);
            if (valueIsNotOneOfAvailable)
                throw new InterviewException(string.Format(
                    "For question {0} was provided selected value {1} as answer. But only following values are allowed: {2}.",
                    FormatQuestionForException(questionId, questionnaire), value, JoinDecimalsWithComma(availableValues)));
        }

        private static void ThrowIfSomeValuesAreNotFromAvailableOptions(Guid questionId, decimal[] values, IQuestionnaire questionnaire)
        {
            IEnumerable<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId);

            bool someValueIsNotOneOfAvailable = values.Any(value => !availableValues.Contains(value));
            if (someValueIsNotOneOfAvailable)
                throw new InterviewException(string.Format(
                    "For question {0} were provided selected values {1} as answer. But only following values are allowed: {2}.",
                    FormatQuestionForException(questionId, questionnaire), JoinDecimalsWithComma(values), JoinDecimalsWithComma(availableValues)));
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
                            => string.Format("{0} : {1}", FormatQuestionForException(questionId, questionnaire), questionnaire.GetCustomValidationExpression(questionId))))));
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
                            => string.Format("{0} : {1}", FormatGroupForException(groupId, questionnaire), questionnaire.GetCustomEnablementConditionForGroup(groupId))))));
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
                            => string.Format("{0} : {1}", FormatQuestionForException(questionId, questionnaire), questionnaire.GetCustomEnablementConditionForQuestion(questionId))))));
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
                            => string.Format("{0}", FormatQuestionForException(questionId, questionnaire))))));
        }

        private void ThrowIfQuestionOrParentGroupIsDisabled(Identity question, IQuestionnaire questionnaire)
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

        private static void ThrowIfAnswerCannotBeUsedAsPropagationCount(Guid questionId, decimal answer, IQuestionnaire questionnaire)
        {
            int maxValue = questionnaire.GetMaxAnswerValueForPropagatingQuestion(questionId);

            bool answerIsNotInteger = answer != (int) answer;
            bool answerIsNegative = answer < 0;
            bool answerExceedsMaxValue = answer > maxValue;

            if (answerIsNotInteger)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question {1} is incorrect because question should propagate groups and answer is not a valid integer.",
                    answer, FormatQuestionForException(questionId, questionnaire)));

            if (answerIsNegative)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question {1} is incorrect because question should propagate groups and answer is negative.",
                    answer, FormatQuestionForException(questionId, questionnaire)));

            if (answerExceedsMaxValue)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question {1} is incorrect because question should propagate groups and answer is greater than max value '{2}'.",
                    answer, FormatQuestionForException(questionId, questionnaire), maxValue));
        }

        private void ThrowIfInterviewStatusIsNotOneOfExpected(params InterviewStatus[] expectedStatuses)
        {
            if (!expectedStatuses.Contains(this.status))
                throw new InterviewException(string.Format(
                    "Interview status is {0}. But one of the following statuses was expected: {1}.",
                    this.status, string.Join(", ", expectedStatuses.Select(expectedStatus => expectedStatus.ToString()))));
        }

        private void ThrowIfStatusNotAllowedToBeChangedWithMetadata(
            InterviewStatus interviewStatus)
        {
            switch (interviewStatus)
            {
                case InterviewStatus.Completed:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.InterviewerAssigned,
                                                                  InterviewStatus.Restarted, InterviewStatus.Restored,
                                                                  InterviewStatus.RejectedBySupervisor);
                    return;
                case InterviewStatus.RejectedBySupervisor:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed, InterviewStatus.Restored,
                        InterviewStatus.InterviewerAssigned,
                        InterviewStatus.ApprovedBySupervisor);
                    return;
                case InterviewStatus.InterviewerAssigned:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(
                        InterviewStatus.SupervisorAssigned, InterviewStatus.Restored,
                        InterviewStatus.RejectedBySupervisor, InterviewStatus.InterviewerAssigned);
                    return;
            }
            throw new InterviewException(string.Format(
                "Status {0} not allowed to be changed with ApplySynchronizationMetadata command",
                interviewStatus));
        }


        private void PerformCustomValidationOfAnsweredQuestionAndDependentQuestions(
            Identity answeredQuestion, IQuestionnaire questionnaire, Func<Identity, object> getAnswer,
            out List<Identity> questionsToBeDeclaredValid, out List<Identity> questionsToBeDeclaredInvalid)
        {
            questionsToBeDeclaredValid = new List<Identity>();
            questionsToBeDeclaredInvalid = new List<Identity>();

            bool? answeredQuestionValidationResult = this.PerformCustomValidationOfQuestion(answeredQuestion, questionnaire, getAnswer);
            switch (answeredQuestionValidationResult)
            {
                case true: questionsToBeDeclaredValid.Add(answeredQuestion); break;
                case false: questionsToBeDeclaredInvalid.Add(answeredQuestion); break;
            }

            List<Identity> dependentQuestionsDeclaredValid;
            List<Identity> dependentQuestionsDeclaredInvalid;
            this.PerformCustomValidationOfDependentQuestions(answeredQuestion, questionnaire, getAnswer, this.GetCountOfPropagatableGroupInstances,
                out dependentQuestionsDeclaredValid, out dependentQuestionsDeclaredInvalid);

            questionsToBeDeclaredValid.AddRange(dependentQuestionsDeclaredValid);
            questionsToBeDeclaredInvalid.AddRange(dependentQuestionsDeclaredInvalid);

            questionsToBeDeclaredValid = this.RemoveQuestionsAlreadyDeclaredValid(questionsToBeDeclaredValid);
            questionsToBeDeclaredInvalid = this.RemoveQuestionsAlreadyDeclaredInvalid(questionsToBeDeclaredInvalid);
        }

        private List<Identity> RemoveQuestionsAlreadyDeclaredValid(IEnumerable<Identity> questionsToBeDeclaredValid)
        {
            return questionsToBeDeclaredValid.Where(question => !this.IsQuestionAnsweredValid(question)).ToList();
        }

        private List<Identity> RemoveQuestionsAlreadyDeclaredInvalid(IEnumerable<Identity> questionsToBeDeclaredInvalid)
        {
            return questionsToBeDeclaredInvalid.Where(question => !this.IsQuestionAnsweredInvalid(question)).ToList();
        }

        private void PerformCustomValidationOfDependentQuestions(Identity question, IQuestionnaire questionnaire,
            Func<Identity, object> getAnswer, Func<Guid, int[], int> getCountOfPropagatableGroupInstances,
            out List<Identity> questionsDeclaredValid, out List<Identity> questionsDeclaredInvalid)
        {
            questionsDeclaredValid = new List<Identity>();
            questionsDeclaredInvalid = new List<Identity>();

            IEnumerable<Guid> dependentQuestionIds = questionnaire.GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(question.Id);
            IEnumerable<Identity> dependentQuestions = this.GetInstancesOfQuestionsWithSameAndDeeperPropagationLevelOrThrow(
                dependentQuestionIds, question.PropagationVector, questionnaire, getCountOfPropagatableGroupInstances);

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

            return this.EvaluateBooleanExpressionOrReturnNullIfExecutionFailsWhenNotEnoughAnswers(
                validationExpression, involvedQuestions, getAnswer, resultIfExecutionFailsWhenAnswersAreEnough: false,
                thisIdentifierQuestionId: question.Id);
        }


        private void DetermineCustomEnablementStateOfDependentGroups(Identity question, IQuestionnaire questionnaire,
            Func<Identity, object> getAnswer, Func<Guid, int[], int> getCountOfPropagatableGroupInstances,
            out List<Identity> groupsToBeDisabled, out List<Identity> groupsToBeEnabled)
        {
            groupsToBeDisabled = new List<Identity>();
            groupsToBeEnabled = new List<Identity>();

            IEnumerable<Guid> dependentGroupIds = questionnaire.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(question.Id);
            IEnumerable<Identity> dependentGroups = this.GetInstancesOfGroupsWithSameAndDeeperPropagationLevelOrThrow(
                dependentGroupIds, question.PropagationVector, questionnaire, getCountOfPropagatableGroupInstances);

            foreach (Identity dependentGroup in dependentGroups)
            {
                PutToCorrespondingListAccordingToEnablementStateChange(dependentGroup, groupsToBeEnabled, groupsToBeDisabled,
                    isNewStateEnabled: this.ShouldGroupBeEnabledByCustomEnablementCondition(dependentGroup, questionnaire, getAnswer),
                    isOldStateEnabled: !this.IsGroupDisabled(dependentGroup));
            }
        }

        private void DetermineCustomEnablementStateOfDependentQuestions(Identity question, IQuestionnaire questionnaire,
            Func<Identity, object> getAnswer, Func<Guid, int[], int> getCountOfPropagatableGroupInstances,
            out List<Identity> questionsToBeDisabled, out List<Identity> questionsToBeEnabled)
        {
            questionsToBeDisabled = new List<Identity>();
            questionsToBeEnabled = new List<Identity>();

            IEnumerable<Guid> dependentQuestionIds = questionnaire.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(question.Id);
            IEnumerable<Identity> dependentQuestions = this.GetInstancesOfQuestionsWithSameAndDeeperPropagationLevelOrThrow(
                dependentQuestionIds, question.PropagationVector, questionnaire, getCountOfPropagatableGroupInstances);

            foreach (Identity dependentQuestion in dependentQuestions)
            {
                PutToCorrespondingListAccordingToEnablementStateChange(dependentQuestion, questionsToBeEnabled, questionsToBeDisabled,
                    isNewStateEnabled: this.ShouldQuestionBeEnabledByCustomEnablementCondition(dependentQuestion, questionnaire, getAnswer),
                    isOldStateEnabled: !this.IsQuestionDisabled(dependentQuestion));
            }
        }

        private void DetermineCustomEnablementStateOfGroupsInitializedByIncreasedPropagation(
            IEnumerable<Guid> idsOfGroupsBeingPropagated, int propagationCount, int[] outerScopePropagationVector, IQuestionnaire questionnaire,
            Func<Identity, object> getAnswer, Func<Guid, int[], int> getCountOfPropagatableGroupInstances,
            out List<Identity> groupsToBeDisabled, out List<Identity> groupsToBeEnabled)
        {
            groupsToBeDisabled = new List<Identity>();
            groupsToBeEnabled = new List<Identity>();

            foreach (Guid idOfGroupBeingPropagated in idsOfGroupsBeingPropagated)
            {
                int oldPropagationCount = this.GetCountOfPropagatableGroupInstances(idOfGroupBeingPropagated, outerScopePropagationVector);

                bool isPropagationCountBeingIncreased = propagationCount > oldPropagationCount;
                if (!isPropagationCountBeingIncreased)
                    continue;

                int indexOfGroupBeingPropagatedInPropagationVector = GetIndexOfPropagatedGroupInPropagationVector(idOfGroupBeingPropagated, questionnaire);

                IEnumerable<Guid> affectedGroupIds = questionnaire.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(idOfGroupBeingPropagated);

                IEnumerable<Identity> affectedGroups = this
                    .GetInstancesOfGroupsWithSameAndDeeperPropagationLevelOrThrow(
                        affectedGroupIds, outerScopePropagationVector, questionnaire, getCountOfPropagatableGroupInstances)
                    .Where(group => IsInstanceBeingInitializedByIncreasedPropagation(group.PropagationVector, oldPropagationCount, indexOfGroupBeingPropagatedInPropagationVector));

                foreach (Identity group in affectedGroups)
                {
                    PutToCorrespondingListAccordingToEnablementStateChange(group, groupsToBeEnabled, groupsToBeDisabled,
                        isNewStateEnabled: this.ShouldGroupBeEnabledByCustomEnablementCondition(group, questionnaire, getAnswer),
                        isOldStateEnabled: true);
                }
            }
        }

        private void DetermineCustomEnablementStateOfQuestionsInitializedByIncreasedPropagation(
            IEnumerable<Guid> idsOfGroupsBeingPropagated, int propagationCount, int[] outerScopePropagationVector, IQuestionnaire questionnaire,
            Func<Identity, object> getAnswer, Func<Guid, int[], int> getCountOfPropagatableGroupInstances,
            out List<Identity> questionsToBeDisabled, out List<Identity> questionsToBeEnabled)
        {
            questionsToBeDisabled = new List<Identity>();
            questionsToBeEnabled = new List<Identity>();

            foreach (Guid idOfGroupBeingPropagated in idsOfGroupsBeingPropagated)
            {
                int oldPropagationCount = this.GetCountOfPropagatableGroupInstances(idOfGroupBeingPropagated, outerScopePropagationVector);

                bool isPropagationCountBeingIncreased = propagationCount > oldPropagationCount;
                if (!isPropagationCountBeingIncreased)
                    continue;

                int indexOfGroupBeingPropagatedInPropagationVector = GetIndexOfPropagatedGroupInPropagationVector(idOfGroupBeingPropagated, questionnaire);

                IEnumerable<Guid> affectedQuestionIds = questionnaire.GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(idOfGroupBeingPropagated);

                IEnumerable<Identity> affectedQuestions = this
                    .GetInstancesOfQuestionsWithSameAndDeeperPropagationLevelOrThrow(
                        affectedQuestionIds, outerScopePropagationVector, questionnaire, getCountOfPropagatableGroupInstances)
                    .Where(group => IsInstanceBeingInitializedByIncreasedPropagation(group.PropagationVector, oldPropagationCount, indexOfGroupBeingPropagatedInPropagationVector));

                foreach (Identity question in affectedQuestions)
                {
                    PutToCorrespondingListAccordingToEnablementStateChange(question, questionsToBeEnabled, questionsToBeDisabled,
                        isNewStateEnabled: this.ShouldQuestionBeEnabledByCustomEnablementCondition(question, questionnaire, getAnswer),
                        isOldStateEnabled: true);
                }
            }
        }

        private bool ShouldGroupBeEnabledByCustomEnablementCondition(Identity group, IQuestionnaire questionnaire, Func<Identity, object> getAnswer)
        {
            return this.ShouldBeEnabledByCustomEnablementCondition(
                questionnaire.GetCustomEnablementConditionForGroup(group.Id),
                group.PropagationVector,
                questionnaire.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(group.Id),
                questionnaire,
                getAnswer);
        }

        private bool ShouldQuestionBeEnabledByCustomEnablementCondition(Identity question, IQuestionnaire questionnaire, Func<Identity, object> getAnswer)
        {
            return this.ShouldBeEnabledByCustomEnablementCondition(
                questionnaire.GetCustomEnablementConditionForQuestion(question.Id),
                question.PropagationVector,
                questionnaire.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(question.Id),
                questionnaire,
                getAnswer);
        }

        private bool ShouldBeEnabledByCustomEnablementCondition(string enablementCondition, int[] propagationVector, IEnumerable<Guid> involvedQuestionIds, IQuestionnaire questionnaire, Func<Identity, object> getAnswer)
        {
            const bool ShouldBeEnabledIfSomeInvolvedQuestionsAreNotAnswered = false;

            IEnumerable<Identity> involvedQuestions = GetInstancesOfQuestionsWithSameAndUpperPropagationLevelOrThrow(involvedQuestionIds, propagationVector, questionnaire);

            return this.EvaluateBooleanExpressionOrReturnNullIfExecutionFailsWhenNotEnoughAnswers(
                enablementCondition, involvedQuestions, getAnswer, resultIfExecutionFailsWhenAnswersAreEnough: true)
                ?? ShouldBeEnabledIfSomeInvolvedQuestionsAreNotAnswered;
        }

        private static void PutToCorrespondingListAccordingToEnablementStateChange(
            Identity entity, List<Identity> entitiesToBeEnabled, List<Identity> entitiesToBeDisabled, bool isNewStateEnabled, bool isOldStateEnabled)
        {
            if (isNewStateEnabled && !isOldStateEnabled)
            {
                entitiesToBeEnabled.Add(entity);
            }

            if (!isNewStateEnabled && isOldStateEnabled)
            {
                entitiesToBeDisabled.Add(entity);
            }
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
            IEnumerable<Guid> questionIds, int[] propagationVector, IQuestionnaire questionnare, Func<Guid, int[], int> getCountOfPropagatableGroupInstances)
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

                IEnumerable<int[]> questionPropagationVectors = this.ExtendPropagationVector(
                    propagationVector, questionPropagationLevel, parentPropagatableGroupsStartingFromTop, getCountOfPropagatableGroupInstances);

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
            IEnumerable<Guid> groupIds, int[] propagationVector, IQuestionnaire questionnare, Func<Guid, int[], int> getCountOfPropagatableGroupInstances)
        {
            int vectorPropagationLevel = propagationVector.Length;

            foreach (Guid groupId in groupIds)
            {
                int groupPropagationLevel = questionnare.GetPropagationLevelForGroup(groupId);

                if (groupPropagationLevel < vectorPropagationLevel)
                    throw new InterviewException(string.Format(
                        "Group {0} expected to have propagation level not upper than {1} but it is {2}.",
                        FormatGroupForException(groupId, questionnare), vectorPropagationLevel, groupPropagationLevel));

                Guid[] propagatableGroupsStartingFromTop = questionnare.GetParentPropagatableGroupsAndGroupItselfIfPropagatableStartingFromTop(groupId).ToArray();
                IEnumerable<int[]> groupPropagationVectors = this.ExtendPropagationVector(
                    propagationVector, groupPropagationLevel, propagatableGroupsStartingFromTop, getCountOfPropagatableGroupInstances);

                foreach (int[] groupPropagationVector in groupPropagationVectors)
                {
                    yield return new Identity(groupId, groupPropagationVector);
                }
            }
        }

        private static List<Identity> GetGroupsToBeDisabledInJustCreatedInterview(IQuestionnaire questionnaire)
        {
            return questionnaire
                .GetAllGroupsWithNotEmptyCustomEnablementConditions()
                .Where(groupId => !questionnaire.IsGroupPropagatable(groupId))
                .Where(groupId => !IsGroupUnderPropagatableGroup(questionnaire, groupId))
                .Select(groupId => new Identity(groupId, EmptyPropagationVector))
                .ToList();
        }

        private static List<Identity> GetQuestionsToBeDisabledInJustCreatedInterview(IQuestionnaire questionnaire)
        {
            return questionnaire
                .GetAllQuestionsWithNotEmptyCustomEnablementConditions()
                .Where(questionId => !IsQuestionUnderPropagatableGroup(questionnaire, questionId))
                .Select(questionId => new Identity(questionId, EmptyPropagationVector))
                .ToList();
        }

        private static bool IsGroupUnderPropagatableGroup(IQuestionnaire questionnaire, Guid groupId)
        {
            return questionnaire.GetParentPropagatableGroupsAndGroupItselfIfPropagatableStartingFromTop(groupId).Any();
        }

        private static bool IsQuestionUnderPropagatableGroup(IQuestionnaire questionnaire, Guid questionId)
        {
            return questionnaire.GetParentPropagatableGroupsForQuestionStartingFromTop(questionId).Any();
        }

        private List<Identity> GetAnswersToRemoveIfPropagationCountIsBeingDecreased(IEnumerable<Guid> idsOfGroupsBeingPropagated, int propagationCount, int[] outerScopePropagationVector, IQuestionnaire questionnaire)
        {
            return idsOfGroupsBeingPropagated
                .SelectMany(idOfGroupBeingPropagated => this.GetAnswersToRemoveIfPropagationCountIsBeingDecreased(idOfGroupBeingPropagated, propagationCount, outerScopePropagationVector, questionnaire))
                .ToList();
        }

        private IEnumerable<Identity> GetAnswersToRemoveIfPropagationCountIsBeingDecreased(Guid idOfGroupBeingPropagated, int propagationCount, int[] outerScopePropagationVector, IQuestionnaire questionnaire)
        {
            bool isPropagationCountBeingDecreased = propagationCount < this.GetCountOfPropagatableGroupInstances(idOfGroupBeingPropagated, outerScopePropagationVector);
            if (!isPropagationCountBeingDecreased)
                return Enumerable.Empty<Identity>();

            int indexOfGroupBeingPropagatedInPropagationVector = GetIndexOfPropagatedGroupInPropagationVector(idOfGroupBeingPropagated, questionnaire);

            IEnumerable<Guid> underlyingQuestionIds = questionnaire.GetAllUnderlyingQuestions(idOfGroupBeingPropagated);

            IEnumerable<Identity> underlyingQuestionInstances = this.GetInstancesOfQuestionsWithSameAndDeeperPropagationLevelOrThrow(
                underlyingQuestionIds, outerScopePropagationVector, questionnaire, this.GetCountOfPropagatableGroupInstances);

            return
                from question in underlyingQuestionInstances
                where this.WasQuestionAnswered(question)
                where IsInstanceBeingRemovedByDecreasedPropagation(question.PropagationVector, propagationCount, indexOfGroupBeingPropagatedInPropagationVector)
                select question;
        }

        private static bool IsInstanceBeingInitializedByIncreasedPropagation(int[] instancePropagationVector, int oldPropagationCount, int indexOfGroupBeingPropagatedInPropagationVector)
        {
            return instancePropagationVector[indexOfGroupBeingPropagatedInPropagationVector] >= oldPropagationCount;
        }

        private static bool IsInstanceBeingRemovedByDecreasedPropagation(int[] instancePropagationVector, int newPropagationCount, int indexOfGroupBeingPropagatedInPropagationVector)
        {
            return instancePropagationVector[indexOfGroupBeingPropagatedInPropagationVector] >= newPropagationCount;
        }

        private static int GetIndexOfPropagatedGroupInPropagationVector(Guid propagatedGroup, IQuestionnaire questionnaire)
        {
            return questionnaire
                .GetParentPropagatableGroupsAndGroupItselfIfPropagatableStartingFromTop(propagatedGroup)
                .ToList()
                .IndexOf(propagatedGroup);
        }


        private bool? EvaluateBooleanExpressionOrReturnNullIfExecutionFailsWhenNotEnoughAnswers(string expression, IEnumerable<Identity> involvedQuestions,
            Func<Identity, object> getAnswer, bool? resultIfExecutionFailsWhenAnswersAreEnough, Guid? thisIdentifierQuestionId = null)
        {
            Dictionary<Guid, object> involvedAnswers = involvedQuestions.ToDictionary(
                involvedQuestion => involvedQuestion.Id,
                involvedQuestion => getAnswer(involvedQuestion));

            bool isSpecialThisIdentifierSupportedByExpression = thisIdentifierQuestionId.HasValue;

            var mapIdentifierToQuestionId = isSpecialThisIdentifierSupportedByExpression
                ? (Func<string, Guid>)(identifier => GetQuestionIdByExpressionIdentifierIncludingThis(identifier, thisIdentifierQuestionId.Value))
                : (Func<string, Guid>)(identifier => GetQuestionIdByExpressionIdentifierExcludingThis(identifier));

            try
            {
                return this.ExpressionProcessor.EvaluateBooleanExpression(expression,
                    getValueForIdentifier: identifier => involvedAnswers[mapIdentifierToQuestionId(identifier)]);
            }
            catch (Exception exception)
            {
                bool areAllInvolvedQuestionsAnswered = involvedAnswers.Values.All(answer => answer != null);

                if (areAllInvolvedQuestionsAnswered)
                {
                    this.Logger.Warn(
                        string.Format("Failed to evaluate boolean expression '{0}' which has all involved answers given.", expression),
                        exception);
                }
                else
                {
                    this.Logger.Info(
                        string.Format("Failed to evaluate boolean expression '{0}' which has some involved answers missing.", expression),
                        exception);
                }

                return areAllInvolvedQuestionsAnswered
                    ? resultIfExecutionFailsWhenAnswersAreEnough
                    : null;
            }
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

        private bool IsQuestionAnsweredValid(Identity question)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(question.Id, question.PropagationVector);

            return this.validAnsweredQuestions.Contains(questionKey);
        }

        private bool IsQuestionAnsweredInvalid(Identity question)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(question.Id, question.PropagationVector);

            return this.invalidAnsweredQuestions.Contains(questionKey);
        }

        private bool HasInvalidAnswers()
        {
            return this.invalidAnsweredQuestions.Any();
        }

        private bool HasNotAnsweredMandatoryQuestions(IQuestionnaire questionnaire)
        {
            IEnumerable<Guid> mandatoryQuestionIds = questionnaire.GetAllMandatoryQuestions();
            IEnumerable<Identity> mandatoryQuestions = this.GetInstancesOfQuestionsWithSameAndDeeperPropagationLevelOrThrow(
                mandatoryQuestionIds, EmptyPropagationVector, questionnaire, this.GetCountOfPropagatableGroupInstances);

            return mandatoryQuestions.Any(
                question => !this.WasQuestionAnswered(question) && !this.IsQuestionOrParentGroupDisabled(question, questionnaire, this.IsGroupDisabled, this.IsQuestionDisabled));
        }

        private bool IsQuestionOrParentGroupDisabled(Identity question, IQuestionnaire questionnaire, Func<Identity, bool> isGroupDisabled, Func<Identity, bool> isQuestionDisabled)
        {
            if (isQuestionDisabled(question))
                return true;

            IEnumerable<Guid> parentGroupIds = questionnaire.GetAllParentGroupsForQuestion(question.Id);
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperPropagationLevelOrThrow(parentGroupIds, question.PropagationVector, questionnaire);

            return parentGroups.Any(isGroupDisabled);
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

        private bool WasQuestionAnswered(Identity question)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(question.Id, question.PropagationVector);

            return this.answeredQuestions.Contains(questionKey);
        }

        private object GetAnswerSupportedInExpressionsOrNull(Identity question)
        {
            string questionKey = ConvertIdAndPropagationVectorToString(question.Id, question.PropagationVector);

            return this.answersSupportedInExpressions.ContainsKey(questionKey)
                ? this.answersSupportedInExpressions[questionKey]
                : null;
        }


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
        private IEnumerable<int[]> ExtendPropagationVector(int[] propagationVector, int length, Guid[] propagatableGroupsStartingFromTop,
            Func<Guid, int[], int> getCountOfPropagatableGroupInstances)
        {
            if (length < propagationVector.Length)
                throw new ArgumentException(string.Format(
                    "Cannot extend vector with length {0} to smaller length {1}.", propagationVector.Length, length));

            if (length == propagationVector.Length)
            {
                yield return propagationVector;
                yield break;
            }

            if (length == propagationVector.Length + 1)
            {
                int countOfInstances =
                    getCountOfPropagatableGroupInstances(propagatableGroupsStartingFromTop.Last(), propagationVector);

                for (int instanceIndex = 0; instanceIndex < countOfInstances; instanceIndex++)
                {
                    yield return ExtendPropagationVectorWithOneValue(propagationVector, instanceIndex);
                }
                yield break;
            }

            throw new NotImplementedException(
                "This method does not support propagated groups inside propagated groups, but may easily support it when needed.");
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
            return string.Format("'{0} [{1}] ({2:N} <{3}>)'",
                GetQuestionTitleForException(question.Id, questionnaire),
                GetQuestionVariableNameForException(question.Id, questionnaire),
                question.Id,
                string.Join("-", question.PropagationVector));
        }

        private static string FormatQuestionForException(Guid questionId, IQuestionnaire questionnaire)
        {
            return string.Format("'{0} [{1}] ({2:N})'",
                GetQuestionTitleForException(questionId, questionnaire),
                GetQuestionVariableNameForException(questionId, questionnaire),
                questionId);
        }

        private static string FormatGroupForException(Identity group, IQuestionnaire questionnaire)
        {
            return string.Format("'{0} ({1:N} <{2}>)'",
                GetGroupTitleForException(group.Id, questionnaire),
                group.Id,
                string.Join("-", group.PropagationVector));
        }

        private static string FormatGroupForException(Guid groupId, IQuestionnaire questionnaire)
        {
            return string.Format("'{0} ({1:N})'",
                GetGroupTitleForException(groupId, questionnaire),
                groupId);
        }

        private static string GetQuestionTitleForException(Guid questionId, IQuestionnaire questionnaire)
        {
            return questionnaire.HasQuestion(questionId)
                ? questionnaire.GetQuestionTitle(questionId) ?? "<<NO QUESTION TITLE>>"
                : "<<MISSING QUESTION>>";
        }

        private static string GetQuestionVariableNameForException(Guid questionId, IQuestionnaire questionnaire)
        {
            return questionnaire.HasQuestion(questionId)
                ? questionnaire.GetQuestionVariableName(questionId) ?? "<<NO VARIABLE NAME>>"
                : "<<MISSING QUESTION>>";
        }

        private static string GetGroupTitleForException(Guid groupId, IQuestionnaire questionnaire)
        {
            return questionnaire.HasGroup(groupId)
                ? questionnaire.GetGroupTitle(groupId) ?? "<<NO GROUP TITLE>>"
                : "<<MISSING GROUP>>";
        }

        private static HashSet<string> ToHashSetOfIdAndPropagationVectorStrings(IEnumerable<InterviewItemId> synchronizationIdentities)
        {
            return new HashSet<string>(
                synchronizationIdentities.Select(question => ConvertIdAndPropagationVectorToString(question.Id, question.PropagationVector)));
        }

        /// <remarks>
        /// The opposite operation (get id or vector from string) should never be performed!
        /// This is one-way transformation. Opposite operation is too slow.
        /// If you need to compactify data and get it back, you should use another datatype, not a string.
        /// </remarks>
        private static string ConvertIdAndPropagationVectorToString(Guid id, int[] propagationVector)
        {
            return string.Format("{0:N}[{1}]", id, string.Join("-", propagationVector));
        }
    }
}
