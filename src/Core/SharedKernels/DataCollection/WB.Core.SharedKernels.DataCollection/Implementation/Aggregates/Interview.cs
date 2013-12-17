using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
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
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Questionnaire;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using InterviewDeleted = WB.Core.SharedKernels.DataCollection.Events.Interview.InterviewDeleted;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Interview : AggregateRootMappedByConvention, ISnapshotable<InterviewState>
    {
        #region Constants

        private static readonly decimal[] EmptyRosterVector = {};

        #endregion

        #region State

        private Guid questionnaireId;
        private long questionnaireVersion;
        private bool wasCompleted;
        private InterviewStatus status;
        private Dictionary<string, object> answersSupportedInExpressions = new Dictionary<string, object>();
        private Dictionary<string, Tuple<Guid, decimal[], decimal[]>> linkedSingleOptionAnswers = new Dictionary<string, Tuple<Guid, decimal[], decimal[]>>();
        private Dictionary<string, Tuple<Guid, decimal[], decimal[][]>> linkedMultipleOptionsAnswers = new Dictionary<string, Tuple<Guid, decimal[], decimal[][]>>();
        private HashSet<string> answeredQuestions = new HashSet<string>();
        private HashSet<string> disabledGroups = new HashSet<string>();
        private HashSet<string> disabledQuestions = new HashSet<string>();
        private Dictionary<string, HashSet<decimal>> rosterGroupInstanceCounts = new Dictionary<string, HashSet<decimal>>();
        private HashSet<string> validAnsweredQuestions = new HashSet<string>();
        private HashSet<string> invalidAnsweredQuestions = new HashSet<string>();

        private void Apply(InterviewCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        private void Apply(InterviewForTestingCreated @event)
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
                : @event.InterviewData.Answers
                    .Where(question => !(question.Answer is GeoPosition || question.Answer is decimal[] || question.Answer is decimal[][]))
                    .ToDictionary(
                        question => ConvertIdAndRosterVectorToString(question.Id, question.PropagationVector),
                        question => question.Answer);

            this.linkedSingleOptionAnswers = @event.InterviewData.Answers == null
                ? new Dictionary<string, Tuple<Guid, decimal[], decimal[]>>()
                : @event.InterviewData.Answers
                    .Where(question => question.Answer is decimal[])
                    .ToDictionary(
                        question => ConvertIdAndRosterVectorToString(question.Id, question.PropagationVector),
                        question => Tuple.Create(question.Id, question.PropagationVector, (decimal[])question.Answer));

            this.linkedMultipleOptionsAnswers = @event.InterviewData.Answers == null
                ? new Dictionary<string, Tuple<Guid, decimal[], decimal[][]>>()
                : @event.InterviewData.Answers
                    .Where(question => question.Answer is decimal[][])
                    .ToDictionary(
                        question => ConvertIdAndRosterVectorToString(question.Id, question.PropagationVector),
                        question => Tuple.Create(question.Id, question.PropagationVector, (decimal[][])question.Answer));

            this.answeredQuestions = new HashSet<string>(
                @event.InterviewData.Answers.Select(question => ConvertIdAndRosterVectorToString(question.Id, question.PropagationVector)));

            this.disabledGroups = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.DisabledGroups);
            this.disabledQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.DisabledQuestions);

            this.rosterGroupInstanceCounts = @event.InterviewData.PropagatedGroupInstanceCounts.ToDictionary(
                pair => ConvertIdAndRosterVectorToString(pair.Key.Id, pair.Key.PropagationVector),
                pair => new HashSet<decimal>(pair.Value));

            this.validAnsweredQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.ValidAnsweredQuestions);
            this.invalidAnsweredQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.InvalidAnsweredQuestions);
        }

        private void Apply(SynchronizationMetadataApplied @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.status = @event.Status;
        }

        internal void Apply(TextQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.Answer;
            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(NumericQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.Answer;
            this.answeredQuestions.Add(questionKey);
        }

        internal void Apply(NumericRealQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.Answer;
            this.answeredQuestions.Add(questionKey);
        }

        internal void Apply(NumericIntegerQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.Answer;
            this.answeredQuestions.Add(questionKey);
        }

        internal void Apply(DateTimeQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.Answer;
            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(SingleOptionQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.SelectedValue;
            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(MultipleOptionsQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions[questionKey] = @event.SelectedValues;
            this.answeredQuestions.Add(questionKey);
        }

        internal void Apply(GeoLocationQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.linkedSingleOptionAnswers[questionKey] = Tuple.Create(@event.QuestionId, @event.PropagationVector, @event.SelectedPropagationVector);
            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.linkedMultipleOptionsAnswers[questionKey] = Tuple.Create(@event.QuestionId, @event.PropagationVector, @event.SelectedPropagationVectors);
            this.answeredQuestions.Add(questionKey);
        }

        private void Apply(AnswerDeclaredValid @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.validAnsweredQuestions.Add(questionKey);
            this.invalidAnsweredQuestions.Remove(questionKey);
        }

        private void Apply(AnswerDeclaredInvalid @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.validAnsweredQuestions.Remove(questionKey);
            this.invalidAnsweredQuestions.Add(questionKey);
        }

        private void Apply(GroupDisabled @event)
        {
            string groupKey = ConvertIdAndRosterVectorToString(@event.GroupId, @event.PropagationVector);

            this.disabledGroups.Add(groupKey);
        }

        private void Apply(GroupEnabled @event)
        {
            string groupKey = ConvertIdAndRosterVectorToString(@event.GroupId, @event.PropagationVector);

            this.disabledGroups.Remove(groupKey);
        }

        private void Apply(QuestionDisabled @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.disabledQuestions.Add(questionKey);
        }

        private void Apply(QuestionEnabled @event)
        {
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.disabledQuestions.Remove(questionKey);
        }

        private void Apply(AnswerCommented @event) {}

        private void Apply(FlagSetToAnswer @event) {}

        private void Apply(FlagRemovedFromAnswer @event) {}

        private void Apply(GroupPropagated @event)
        {
            string rosterGroupKey = ConvertIdAndRosterVectorToString(@event.GroupId, @event.OuterScopePropagationVector);
            var rosterRowInstances = new HashSet<decimal>();

            for (int i = 0; i < @event.Count; i++)
            {
                rosterRowInstances.Add(i);
            }

            this.rosterGroupInstanceCounts[rosterGroupKey] = rosterRowInstances;
        }

        private void Apply(RosterRowAdded @event)
        {
            string rosterGroupKey = ConvertIdAndRosterVectorToString(@event.GroupId, @event.OuterRosterVector);
            var rosterRowInstances = this.rosterGroupInstanceCounts.ContainsKey(rosterGroupKey)
                ? this.rosterGroupInstanceCounts[rosterGroupKey]
                : new HashSet<decimal>();

            rosterRowInstances.Add(@event.RosterInstanceId);

            this.rosterGroupInstanceCounts[rosterGroupKey] = rosterRowInstances;
        }

        private void Apply(RosterRowDeleted @event)
        {
            string rosterGroupKey = ConvertIdAndRosterVectorToString(@event.GroupId, @event.OuterRosterVector);
            this.rosterGroupInstanceCounts[rosterGroupKey].Remove(@event.RosterInstanceId);
        }


        private void Apply(RosterTitleChanged @event)
        {
            
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
            string questionKey = ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.answersSupportedInExpressions.Remove(questionKey);
            this.linkedSingleOptionAnswers.Remove(questionKey);
            this.linkedMultipleOptionsAnswers.Remove(questionKey);
            this.answeredQuestions.Remove(questionKey);
            this.disabledQuestions.Remove(questionKey);
            this.validAnsweredQuestions.Remove(questionKey);
            this.invalidAnsweredQuestions.Remove(questionKey);
        }

        public InterviewState CreateSnapshot()
        {
            return new InterviewState(
                this.questionnaireId,
                this.questionnaireVersion,
                this.status,
                this.answersSupportedInExpressions,
                this.linkedSingleOptionAnswers,
                this.linkedMultipleOptionsAnswers,
                this.answeredQuestions,
                this.disabledGroups,
                this.disabledQuestions,
                this.rosterGroupInstanceCounts,
                this.validAnsweredQuestions,
                this.invalidAnsweredQuestions,
                this.wasCompleted);
        }

        public void RestoreFromSnapshot(InterviewState snapshot)
        {
            this.questionnaireId                = snapshot.QuestionnaireId;
            this.questionnaireVersion           = snapshot.QuestionnaireVersion;
            this.status                         = snapshot.Status;
            this.answersSupportedInExpressions  = snapshot.AnswersSupportedInExpressions;
            this.linkedSingleOptionAnswers      = snapshot.LinkedSingleOptionAnswers;
            this.linkedMultipleOptionsAnswers   = snapshot.LinkedMultipleOptionsAnswers;
            this.answeredQuestions              = snapshot.AnsweredQuestions;
            this.disabledGroups                 = snapshot.DisabledGroups;
            this.disabledQuestions              = snapshot.DisabledQuestions;
            this.rosterGroupInstanceCounts  = snapshot.PropagatedGroupInstanceCounts;
            this.validAnsweredQuestions         = snapshot.ValidAnsweredQuestions;
            this.invalidAnsweredQuestions       = snapshot.InvalidAnsweredQuestions;
            this.wasCompleted                   = snapshot.WasCompleted;
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
        /// Full identity of group or question: id and roster vector.
        /// </summary>
        /// <remarks>
        /// Is used only internally to simplify return of id and roster vector as return value
        /// and to reduce parameters count in calculation methods.
        /// Should not be made public or be used in any form in events or commands.
        /// </remarks>
        private class Identity
        {
            public Guid Id { get; private set; }
            public decimal[] RosterVector { get; private set; }

            public Identity(Guid id, decimal[] rosterVector)
            {
                this.Id = id;
                this.RosterVector = rosterVector;
            }
        }

        private class RosterIdentity
        {
            public Guid GroupId { get; private set; }
            public decimal[] OuterRosterVector { get; private set; }
            public decimal RosterInstanceId { get; private set; }
            public int? SortIndex { get; private set; }

            public RosterIdentity(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex = null)
            {
                this.GroupId = groupId;
                this.OuterRosterVector = outerRosterVector;
                this.RosterInstanceId = rosterInstanceId;
                this.SortIndex = sortIndex;
            }
        }

        #endregion


        /// <remarks>Is used to restore aggregate from event stream.</remarks>
        public Interview() {}

        public Interview(Guid id, Guid userId, Guid questionnaireId, Dictionary<Guid, object> answersToFeaturedQuestions, DateTime answersTime, Guid supervisorId)
            : base(id)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireId);

            List<Identity> initiallyDisabledGroups = GetGroupsToBeDisabledInJustCreatedInterview(questionnaire);
            List<Identity> initiallyDisabledQuestions = GetQuestionsToBeDisabledInJustCreatedInterview(questionnaire);
            List<Identity> initiallyInvalidQuestions = GetQuestionsToBeInvalidInJustCreatedInterview(questionnaire, initiallyDisabledGroups, initiallyDisabledQuestions);

            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            initiallyDisabledGroups.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.RosterVector)));
            initiallyDisabledQuestions.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.RosterVector)));
            initiallyInvalidQuestions.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.RosterVector)));


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
                        this.AnswerTextQuestion(userId, questionId, EmptyRosterVector, answersTime, (string)answer);
                        break;

                    case QuestionType.AutoPropagate:
                        this.AnswerNumericIntegerQuestion(userId, questionId, EmptyRosterVector, answersTime, (int)answer);
                        break;
                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            this.AnswerNumericIntegerQuestion(userId, questionId, EmptyRosterVector, answersTime, (int) answer);
                        else
                            this.AnswerNumericRealQuestion(userId, questionId, EmptyRosterVector, answersTime, (decimal)answer);
                        break;

                    case QuestionType.DateTime:
                        this.AnswerDateTimeQuestion(userId, questionId, EmptyRosterVector, answersTime, (DateTime)answer);
                        break;

                    case QuestionType.SingleOption:
                        this.AnswerSingleOptionQuestion(userId, questionId, EmptyRosterVector, answersTime, (decimal)answer);
                        break;

                    case QuestionType.MultyOption:
                        this.AnswerMultipleOptionsQuestion(userId, questionId, EmptyRosterVector, answersTime, (decimal[])answer);
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

        public Interview(Guid id, Guid userId, Guid questionnaireId, Dictionary<Guid, object> answersToFeaturedQuestions, DateTime answersTime)
            : base(id)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireId);

            List<Identity> initiallyDisabledGroups = GetGroupsToBeDisabledInJustCreatedInterview(questionnaire);
            List<Identity> initiallyDisabledQuestions = GetQuestionsToBeDisabledInJustCreatedInterview(questionnaire);
            List<Identity> initiallyInvalidQuestions = GetQuestionsToBeInvalidInJustCreatedInterview(questionnaire, initiallyDisabledGroups, initiallyDisabledQuestions);

            this.ApplyEvent(new InterviewForTestingCreated(userId, questionnaireId, questionnaire.Version));
            //this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            initiallyDisabledGroups.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.RosterVector)));
            initiallyDisabledQuestions.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.RosterVector)));
            initiallyInvalidQuestions.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.RosterVector)));


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
                        this.AnswerTextQuestion(userId, questionId, EmptyRosterVector, answersTime, (string)answer);
                        break;

                    case QuestionType.AutoPropagate:
                        this.AnswerNumericIntegerQuestion(userId, questionId, EmptyRosterVector, answersTime, (int)answer);
                        break;
                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            this.AnswerNumericIntegerQuestion(userId, questionId, EmptyRosterVector, answersTime, (int)answer);
                        else
                            this.AnswerNumericRealQuestion(userId, questionId, EmptyRosterVector, answersTime, (decimal)answer);
                        break;

                    case QuestionType.DateTime:
                        this.AnswerDateTimeQuestion(userId, questionId, EmptyRosterVector, answersTime, (DateTime)answer);
                        break;

                    case QuestionType.SingleOption:
                        this.AnswerSingleOptionQuestion(userId, questionId, EmptyRosterVector, answersTime, (decimal)answer);
                        break;

                    case QuestionType.MultyOption:
                        this.AnswerMultipleOptionsQuestion(userId, questionId, EmptyRosterVector, answersTime, (decimal[])answer);
                        break;

                    case QuestionType.GpsCoordinates:
                    default:
                        throw new InterviewException(string.Format(
                            "Question {0} has type {1} which is not supported as initial pre-filled question.",
                            questionId, questionType));
                }
            }
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, InterviewStatus interviewStatus, AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, string comments, bool valid)
            : base(id)
        {
            this.ApplySynchronizationMetadata(id, userId, questionnaireId, interviewStatus, featuredQuestionsMeta, comments, valid);
        }

        public void SynchronizeInterview(Guid userId, InterviewSynchronizationDto synchronizedInterview)
        {
            this.ApplyEvent(new InterviewSynchronized(synchronizedInterview));
        }

        public void ApplySynchronizationMetadata(Guid id, Guid userId, Guid questionnaireId, InterviewStatus interviewStatus, AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, string comments, bool valid)
        {
            if (this.status == InterviewStatus.Deleted)
                Restore(userId);
            else
                ThrowIfStatusNotAllowedToBeChangedWithMetadata(interviewStatus);

            ApplyEvent(new SynchronizationMetadataApplied(userId, questionnaireId, 
                                                          interviewStatus,
                                                          featuredQuestionsMeta));

            ApplyEvent(new InterviewStatusChanged(interviewStatus, comments));

            if (valid)
                this.ApplyEvent(new InterviewDeclaredValid());
            else
                this.ApplyEvent(new InterviewDeclaredInvalid());
        }

        public void AnswerTextQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, string answer)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Text);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);
            
            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? answer : this.GetEnabledQuestionAnswerSupportedInExpressions(question);

            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, this.GetRosterInstanceIds, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, answer, questionnaire, this.GetRosterInstanceIds,
                out questionsToBeDisabled, out questionsToBeEnabled);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    groupsToBeDisabled, questionsToBeDisabled, questionnaire, this.GetRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (questionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (questionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };
            Func<Identity, object> getAnswerConcerningDisabling = question => AreEqual(question, answeredQuestion) ? answer : this.GetAnswerSupportedInExpressionsForEnabledOrNull(question, getNewQuestionState);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState,groupsToBeEnabled, questionsToBeEnabled, out answersDeclaredValid, out answersDeclaredInvalid);
            
            this.ApplyEvent(new TextQuestionAnswered(userId, questionId, rosterVector, answerTime, answer));
            
            /*if (questionnaire.IsQuestionHeadOfRoster(questionId))
            {
                this.ApplyEvent(new RosterTitleChanged(userId, questionId, rosterVector, answer));
            }
*/
            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.RosterVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.RosterVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.RosterVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.RosterVector)));
            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.RosterVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.RosterVector)));

            answersForLinkedQuestionsToRemoveByDisabling.ForEach(question => this.ApplyEvent(new AnswerRemoved(question.Id, question.RosterVector)));
        }

        public void AnswerNumericIntegerQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, int answer)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.AutoPropagate, QuestionType.Numeric);
            this.ThrowIfNumericQuestionIsNotInteger(questionId, questionnaire);
            ThrowIfNumericAnswerExceedsMaxValue(questionId, answer, questionnaire);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);

            if (questionnaire.ShouldQuestionSpecifyRosterSize(questionId))
            {
                ThrowIfRosterSizeAnswerIsNegative(questionId, answer, questionnaire);
            }

            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? answer : this.GetEnabledQuestionAnswerSupportedInExpressions(question);

            List<Guid> idsOfRosterGroups = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();
            int rosterSize = idsOfRosterGroups.Any() ? ToRosterSize(answer) : 0;

            Func<Guid, decimal[], bool> isRosterGroup = (groupId, groupOuterScopePropagationVector)
                => idsOfRosterGroups.Contains(groupId)
                && AreEqualRosterVectors(groupOuterScopePropagationVector, rosterVector);

            Func<Guid, decimal[], int> getCountOfRosterGroupInstances = (groupId, groupOuterRosterVector)
                => isRosterGroup(groupId, groupOuterRosterVector)
                    ? rosterSize
                    : this.GetCountOfRosterGroupInstances(groupId, groupOuterRosterVector);

            Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds = (groupId, outerRosterVector)
                => GetRosterInstanceIds(groupId, outerRosterVector, getCountOfRosterGroupInstances);

            List<Identity> answersToRemoveByDecreasedRosterSize, initializedGroupsToBeDisabled, initializedGroupsToBeEnabled,
                initializedQuestionsToBeDisabled, initializedQuestionsToBeEnabled, initializedQuestionsToBeInvalid;
            List<RosterIdentity> rosterInstancesToAdd, rosterInstancesToDelete;
            this.CalculateRosterData(rosterVector, idsOfRosterGroups, rosterSize, questionnaire, getAnswer, getRosterInstanceIds,
                out answersToRemoveByDecreasedRosterSize, out initializedGroupsToBeDisabled, out initializedGroupsToBeEnabled,
                out initializedQuestionsToBeDisabled, out initializedQuestionsToBeEnabled, out initializedQuestionsToBeInvalid,
                out rosterInstancesToAdd, out rosterInstancesToDelete);

            List<Identity> dependentGroupsToBeDisabled, dependentGroupsToBeEnabled, dependentQuestionsToBeDisabled, dependentQuestionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, getRosterInstanceIds,
                out dependentGroupsToBeDisabled, out dependentGroupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, answer, questionnaire, getRosterInstanceIds,
                out dependentQuestionsToBeDisabled, out dependentQuestionsToBeEnabled);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    Enumerable.Concat(initializedGroupsToBeDisabled, dependentGroupsToBeDisabled),
                    Enumerable.Concat(initializedQuestionsToBeDisabled, dependentQuestionsToBeDisabled),
                    questionnaire, getRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (initializedQuestionsToBeDisabled.Any(q => AreEqual(q, question)) || dependentQuestionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (initializedQuestionsToBeEnabled.Any(q => AreEqual(q, question)) || dependentQuestionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };

            Func<Identity, object> getAnswerConcerningDisabling = question => AreEqual(question, answeredQuestion) ? answer : this.GetAnswerSupportedInExpressionsForEnabledOrNull(question, getNewQuestionState);
            
            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState, dependentGroupsToBeEnabled, dependentQuestionsToBeEnabled, out answersDeclaredValid, out answersDeclaredInvalid);


            this.ApplyEvent(new NumericIntegerQuestionAnswered(userId, questionId, rosterVector, answerTime, answer));

            this.ApplyRosterEvents(rosterInstancesToAdd, rosterInstancesToDelete, answersToRemoveByDecreasedRosterSize,
                initializedGroupsToBeDisabled, initializedGroupsToBeEnabled,
                initializedQuestionsToBeDisabled, initializedQuestionsToBeEnabled,
                initializedQuestionsToBeInvalid);

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.RosterVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.RosterVector)));

            dependentGroupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.RosterVector)));
            dependentGroupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.RosterVector)));
            dependentQuestionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.RosterVector)));
            dependentQuestionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.RosterVector)));

            answersForLinkedQuestionsToRemoveByDisabling.ForEach(question => this.ApplyEvent(new AnswerRemoved(question.Id, question.RosterVector)));
        }

        public void AnswerNumericRealQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal answer)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Numeric);
            this.ThrowIfNumericQuestionIsNotReal(questionId, questionnaire);
            ThrowIfNumericAnswerExceedsMaxValue(questionId, answer, questionnaire);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);
            this.ThrowIfAnswerHasMoreDecimalPlacesThenAccepted(questionnaire, questionId, answer);


            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? answer : this.GetEnabledQuestionAnswerSupportedInExpressions(question);

            List<Identity> dependentGroupsToBeDisabled, dependentGroupsToBeEnabled, dependentQuestionsToBeDisabled, dependentQuestionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, GetRosterInstanceIds,
                out dependentGroupsToBeDisabled, out dependentGroupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, answer, questionnaire, this.GetRosterInstanceIds,
                out dependentQuestionsToBeDisabled, out dependentQuestionsToBeEnabled);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    dependentGroupsToBeDisabled,
                    dependentQuestionsToBeDisabled,
                    questionnaire, this.GetRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (dependentQuestionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (dependentQuestionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };

            Func<Identity, object> getAnswerConcerningDisabling = question => AreEqual(question, answeredQuestion) ? answer : this.GetAnswerSupportedInExpressionsForEnabledOrNull(question, getNewQuestionState);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState, dependentGroupsToBeEnabled, dependentQuestionsToBeEnabled, out answersDeclaredValid, out answersDeclaredInvalid);



            this.ApplyEvent(new NumericRealQuestionAnswered(userId, questionId, rosterVector, answerTime, answer));

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.RosterVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.RosterVector)));

            dependentGroupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.RosterVector)));
            dependentGroupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.RosterVector)));
            dependentQuestionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.RosterVector)));
            dependentQuestionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.RosterVector)));

            answersForLinkedQuestionsToRemoveByDisabling.ForEach(question => this.ApplyEvent(new AnswerRemoved(question.Id, question.RosterVector)));
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, DateTime answer)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.DateTime);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);



            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? answer : this.GetEnabledQuestionAnswerSupportedInExpressions(question);
            
            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, this.GetRosterInstanceIds, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, answer, questionnaire, this.GetRosterInstanceIds,
                out questionsToBeDisabled, out questionsToBeEnabled);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    groupsToBeDisabled, questionsToBeDisabled, questionnaire, this.GetRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (questionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (questionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };
            Func<Identity, object> getAnswerConcerningDisabling = 
                question => AreEqual(question, answeredQuestion) ? answer : this.GetAnswerSupportedInExpressionsForEnabledOrNull(question, getNewQuestionState);



            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState,groupsToBeEnabled,questionsToBeEnabled,out answersDeclaredValid, out answersDeclaredInvalid);

            this.ApplyEvent(new DateTimeQuestionAnswered(userId, questionId, rosterVector, answerTime, answer));

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.RosterVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.RosterVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.RosterVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.RosterVector)));
            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.RosterVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.RosterVector)));

            answersForLinkedQuestionsToRemoveByDisabling.ForEach(question => this.ApplyEvent(new AnswerRemoved(question.Id, question.RosterVector)));
        }

        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal selectedValue)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.SingleOption);
            ThrowIfValueIsNotOneOfAvailableOptions(questionId, selectedValue, questionnaire);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);

            

            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? selectedValue : this.GetEnabledQuestionAnswerSupportedInExpressions(question);
            
            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, this.GetRosterInstanceIds, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, selectedValue, questionnaire, this.GetRosterInstanceIds,
                out questionsToBeDisabled, out questionsToBeEnabled);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    groupsToBeDisabled, questionsToBeDisabled, questionnaire, this.GetRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (questionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (questionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };

            Func<Identity, object> getAnswerConcerningDisabling = question => AreEqual(question, answeredQuestion) ? selectedValue : this.GetAnswerSupportedInExpressionsForEnabledOrNull(question, getNewQuestionState);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState,groupsToBeEnabled,questionsToBeEnabled,out answersDeclaredValid, out answersDeclaredInvalid);



            this.ApplyEvent(new SingleOptionQuestionAnswered(userId, questionId, rosterVector, answerTime, selectedValue));

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.RosterVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.RosterVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.RosterVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.RosterVector)));
            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.RosterVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.RosterVector)));

            answersForLinkedQuestionsToRemoveByDisabling.ForEach(question => this.ApplyEvent(new AnswerRemoved(question.Id, question.RosterVector)));
        }

        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[] selectedValues)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.MultyOption);
            ThrowIfSomeValuesAreNotFromAvailableOptions(questionId, selectedValues, questionnaire);
            ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(questionId, selectedValues.Length, questionnaire);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);



            Func<Identity, object> getAnswer = question => AreEqual(question, answeredQuestion) ? selectedValues : this.GetEnabledQuestionAnswerSupportedInExpressions(question);
            
            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;
            this.DetermineCustomEnablementStateOfDependentGroups(
                answeredQuestion, questionnaire, getAnswer, this.GetRosterInstanceIds, out groupsToBeDisabled, out groupsToBeEnabled);
            this.DetermineCustomEnablementStateOfDependentQuestions(
                answeredQuestion, selectedValues, questionnaire, this.GetRosterInstanceIds,
                out questionsToBeDisabled, out questionsToBeEnabled);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    groupsToBeDisabled, questionsToBeDisabled, questionnaire, this.GetRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (questionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (questionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };

            Func<Identity, object> getAnswerConcerningDisabling =
                question =>
                {
                    if (AreEqual(question, answeredQuestion))
                    {
                        return selectedValues.Any() ? selectedValues : null;
                    }
                    else
                    {
                        return this.GetAnswerSupportedInExpressionsForEnabledOrNull(question, getNewQuestionState);
                    }
                };

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState,groupsToBeEnabled,questionsToBeEnabled,out answersDeclaredValid, out answersDeclaredInvalid);



            this.ApplyEvent(new MultipleOptionsQuestionAnswered(userId, questionId, rosterVector, answerTime, selectedValues));

            answersDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.RosterVector)));
            answersDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.RosterVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.RosterVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.RosterVector)));
            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.RosterVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.RosterVector)));

            answersForLinkedQuestionsToRemoveByDisabling.ForEach(question => this.ApplyEvent(new AnswerRemoved(question.Id, question.RosterVector)));
        }

        public void AnswerGeoLocationQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, 
            double latitude, double longitude, double accuracy, DateTimeOffset timestamp)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.GpsCoordinates);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);



            this.ApplyEvent(new GeoLocationQuestionAnswered(userId, questionId, rosterVector, answerTime, latitude, longitude, accuracy, timestamp));

            this.ApplyEvent(new AnswerDeclaredValid(questionId, rosterVector));
        }

        public void AnswerSingleOptionLinkedQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[] selectedPropagationVector)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);
            
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.SingleOption);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);

            Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
            var answeredLinkedQuestion = new Identity(linkedQuestionId, selectedPropagationVector);

            this.ThrowIfRosterVectorIsIncorrect(linkedQuestionId, selectedPropagationVector, questionnaire);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredLinkedQuestion, questionnaire);
            this.ThrowIfLinkedQuestionDoesNotHaveAnswer(answeredQuestion, answeredLinkedQuestion, questionnaire);



            this.ApplyEvent(new SingleOptionLinkedQuestionAnswered(userId, questionId, rosterVector, answerTime, selectedPropagationVector));

            this.ApplyEvent(new AnswerDeclaredValid(questionId, rosterVector));
        }

        public void AnswerMultipleOptionsLinkedQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[][] selectedPropagationVectors)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.MultyOption);
            this.ThrowIfQuestionOrParentGroupIsDisabled(answeredQuestion, questionnaire);
            
            Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
            foreach (var answeredLinkedQuestion in selectedPropagationVectors.Select(selectedRosterVector => new Identity(linkedQuestionId, selectedRosterVector)))
            {
                this.ThrowIfRosterVectorIsIncorrect(linkedQuestionId, answeredLinkedQuestion.RosterVector, questionnaire);    
                this.ThrowIfQuestionOrParentGroupIsDisabled(answeredLinkedQuestion, questionnaire);
                this.ThrowIfLinkedQuestionDoesNotHaveAnswer(answeredQuestion, answeredLinkedQuestion, questionnaire);
            }
            ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(questionId, selectedPropagationVectors.Length, questionnaire);


            this.ApplyEvent(new MultipleOptionsLinkedQuestionAnswered(userId, questionId, rosterVector, answerTime, selectedPropagationVectors));

            this.ApplyEvent(new AnswerDeclaredValid(questionId, rosterVector));
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
                var availableRosterLevels = this.AvailableRosterLevelsForGroup(questionnaire, groupWithNotEmptyCustomEnablementCondition);

                foreach (var availableRosterLevel in availableRosterLevels)
                {
                    Identity groupIdAtInterview = new Identity(groupWithNotEmptyCustomEnablementCondition, availableRosterLevel);

                    PutToCorrespondingListAccordingToEnablementStateChange(groupIdAtInterview, groupsToBeEnabled, groupsToBeDisabled,
                        isNewStateEnabled: this.ShouldGroupBeEnabledByCustomEnablementCondition(groupIdAtInterview, questionnaire,
                            this.GetEnabledQuestionAnswerSupportedInExpressions),
                        isOldStateEnabled: !this.IsGroupDisabled(groupIdAtInterview));
                }
            }

            foreach (var questionWithNotEmptyEnablementCondition in questionnaire.GetAllQuestionsWithNotEmptyCustomEnablementConditions())
            {
                var availableRosterLevels = this.AvailableRosterLevelsForQuestion(questionnaire, questionWithNotEmptyEnablementCondition);

                foreach (var availableRosterLevel in availableRosterLevels)
                {
                    Identity questionIdAtInterview = new Identity(questionWithNotEmptyEnablementCondition, availableRosterLevel);

                    PutToCorrespondingListAccordingToEnablementStateChange(questionIdAtInterview, questionsToBeEnabled,
                        questionsToBeDisabled,
                        isNewStateEnabled:
                            this.ShouldQuestionBeEnabledByCustomEnablementCondition(questionIdAtInterview, questionnaire,
                                this.GetEnabledQuestionAnswerSupportedInExpressions),
                        isOldStateEnabled: !this.IsQuestionDisabled(questionIdAtInterview));
                }
            }

            foreach (var questionWithNotEmptyValidationExpression in questionnaire.GetAllQuestionsWithNotEmptyValidationExpressions())
            {
                var availableRosterLevels = this.AvailableRosterLevelsForQuestion(questionnaire,
                    questionWithNotEmptyValidationExpression);

                foreach (var availableRosterLevel in availableRosterLevels)
                {
                    Identity questionIdAtInterview = new Identity(questionWithNotEmptyValidationExpression, availableRosterLevel);


                    if (IsQuestionOrParentGroupDisabled(questionIdAtInterview, questionnaire, (question) => groupsToBeDisabled.Any(q => AreEqual(q, question)), (question) => questionsToBeDisabled.Any(q => AreEqual(q, question))))
                        continue;

                    string questionKey = ConvertIdAndRosterVectorToString(questionIdAtInterview.Id, questionIdAtInterview.RosterVector);

                    if(!this.answeredQuestions.Contains(questionKey))
                        continue;

                    bool? dependentQuestionValidationResult = this.PerformValidationOfQuestion(questionIdAtInterview, questionnaire,
                        this.GetEnabledQuestionAnswerSupportedInExpressions, a => null);

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

            foreach (var mandatoryQuestion in questionnaire.GetAllMandatoryQuestions())
            {
                var availableRosterLevels = this.AvailableRosterLevelsForQuestion(questionnaire, mandatoryQuestion);

                foreach (var availableRosterLevel in availableRosterLevels)
                {
                    Identity questionIdAtInterview = new Identity(mandatoryQuestion, availableRosterLevel);

                    if (questionsDeclaredInvalid.Contains(questionIdAtInterview) || questionsDeclaredInvalid.Contains(questionIdAtInterview))
                        continue;

                    string questionKey = ConvertIdAndRosterVectorToString(questionIdAtInterview.Id, questionIdAtInterview.RosterVector);

                    if (!this.answeredQuestions.Contains(questionKey))
                        continue;

                    questionsDeclaredValid.Add(questionIdAtInterview);
                }
            }

            bool isInterviewInvalid = this.HasInvalidAnswers();

            questionsDeclaredValid.ForEach(question => this.ApplyEvent(new AnswerDeclaredValid(question.Id, question.RosterVector)));
            questionsDeclaredInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.RosterVector)));

            groupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.RosterVector)));
            groupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.RosterVector)));

            questionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.RosterVector)));
            questionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.RosterVector)));

            if (!isInterviewInvalid)
            {
                this.ApplyEvent(new InterviewDeclaredValid());
            }
        }

        public void CommentAnswer(Guid userId, Guid questionId, decimal[] rosterVector, DateTime commentTime, string comment)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);

            this.ApplyEvent(new AnswerCommented(userId, questionId, rosterVector, commentTime, comment));
        }

        public void SetFlagToAnswer(Guid userId, Guid questionId, decimal[] rosterVector)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);

            this.ApplyEvent(new FlagSetToAnswer(userId, questionId, rosterVector));
        }

        public void RemoveFlagFromAnswer(Guid userId, Guid questionId, decimal[] rosterVector)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(questionId, rosterVector, questionnaire);

            this.ApplyEvent(new FlagRemovedFromAnswer(userId, questionId, rosterVector));
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
            this.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.InterviewerAssigned, InterviewStatus.Restarted, InterviewStatus.RejectedBySupervisor);
            
            /*IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);*/
            bool isInterviewInvalid = this.HasInvalidAnswers() /*|| this.HasNotAnsweredMandatoryQuestions(questionnaire)*/;

            this.ApplyEvent(new InterviewCompleted(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Completed, comment));

            this.ApplyEvent(isInterviewInvalid
                ? new InterviewDeclaredInvalid() as object
                : new InterviewDeclaredValid() as object);
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



        private void ApplyRosterEvents(
            List<RosterIdentity> rosterInstancesToAdd, List<RosterIdentity> rosterInstancesToDelete,
            List<Identity> answersToRemoveByDecreasedRosterSize,
            List<Identity> initializedGroupsToBeDisabled, List<Identity> initializedGroupsToBeEnabled,
            List<Identity> initializedQuestionsToBeDisabled, List<Identity> initializedQuestionsToBeEnabled,
            List<Identity> initializedQuestionsToBeInvalid)
        {
            rosterInstancesToAdd.ForEach(roster => this.ApplyEvent(new RosterRowAdded(roster.GroupId, roster.OuterRosterVector, roster.RosterInstanceId, roster.SortIndex)));
            rosterInstancesToDelete.ForEach(roster => this.ApplyEvent(new RosterRowDeleted(roster.GroupId, roster.OuterRosterVector, roster.RosterInstanceId)));

            answersToRemoveByDecreasedRosterSize.ForEach(question => this.ApplyEvent(new AnswerRemoved(question.Id, question.RosterVector)));

            initializedGroupsToBeDisabled.ForEach(group => this.ApplyEvent(new GroupDisabled(group.Id, group.RosterVector)));
            initializedGroupsToBeEnabled.ForEach(group => this.ApplyEvent(new GroupEnabled(group.Id, group.RosterVector)));
            initializedQuestionsToBeDisabled.ForEach(question => this.ApplyEvent(new QuestionDisabled(question.Id, question.RosterVector)));
            initializedQuestionsToBeEnabled.ForEach(question => this.ApplyEvent(new QuestionEnabled(question.Id, question.RosterVector)));
            initializedQuestionsToBeInvalid.ForEach(question => this.ApplyEvent(new AnswerDeclaredInvalid(question.Id, question.RosterVector)));
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

        private void ThrowIfQuestionDoesNotExist(Guid questionId, IQuestionnaire questionnaire)
        {
            if (!questionnaire.HasQuestion(questionId))
                throw new InterviewException(string.Format("Question with id '{0}' is not found.", questionId));
        }

        private void ThrowIfRosterVectorIsIncorrect(Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire)
        {
            ThrowIfRosterVectorIsNull(questionId, rosterVector, questionnaire);

            Guid[] parentRosterGroupIdsStartingFromTop = questionnaire.GetParentRosterGroupsForQuestionStartingFromTop(questionId).ToArray();

            ThrowIfRosterVectorLengthDoesNotCorrespondToParentRosterGroupsCount(questionId, rosterVector, parentRosterGroupIdsStartingFromTop, questionnaire);

            this.ThrowIfSomeOfRosterVectorValuesAreInvalid(questionId, rosterVector, parentRosterGroupIdsStartingFromTop, questionnaire);
        }

        private static void ThrowIfRosterVectorIsNull(Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire)
        {
            if (rosterVector == null)
                throw new InterviewException(string.Format(
                    "Roster information for question {0} is missing. Roster vector cannot be null.",
                    FormatQuestionForException(questionId, questionnaire)));
        }

        private static void ThrowIfRosterVectorLengthDoesNotCorrespondToParentRosterGroupsCount(
            Guid questionId, decimal[] rosterVector, Guid[] parentRosterGroups, IQuestionnaire questionnaire)
        {
            if (rosterVector.Length != parentRosterGroups.Length)
                throw new InterviewException(string.Format(
                    "Roster information for question {0} is incorrect. " +
                    "Roster vector has {1} elements, but parent roster groups count is {2}.",
                    FormatQuestionForException(questionId, questionnaire), rosterVector.Length, parentRosterGroups.Length));
        }

        private void ThrowIfSomeOfRosterVectorValuesAreInvalid(
            Guid questionId, decimal[] rosterVector, Guid[] parentRosterGroupIdsStartingFromTop, IQuestionnaire questionnaire)
        {
            for (int indexOfRosterVectorElement = 0; indexOfRosterVectorElement < rosterVector.Length; indexOfRosterVectorElement++)
            {
                decimal rosterGroupInstanceIndex = rosterVector[indexOfRosterVectorElement];
                Guid rosterGroupId = parentRosterGroupIdsStartingFromTop[indexOfRosterVectorElement];

                int rosterGroupOuterScopeRosterLevel = indexOfRosterVectorElement;
                decimal[] rosterGroupOuterScopeRosterVector = ShrinkRosterVector(rosterVector, rosterGroupOuterScopeRosterLevel);
                int countOfRosterGroupInstances = this.GetCountOfRosterGroupInstances(
                    rosterGroupId: rosterGroupId,
                    outerScopeRosterVector: rosterGroupOuterScopeRosterVector);

                if (rosterGroupInstanceIndex < 0)
                    throw new InterviewException(string.Format(
                        "Roster information for question {0} is incorrect. " +
                        "Roster vector element with index [{1}] is negative.",
                        FormatQuestionForException(questionId, questionnaire), indexOfRosterVectorElement));

                if (rosterGroupInstanceIndex >= countOfRosterGroupInstances)
                    throw new InterviewException(string.Format(
                        "Roster information for question {0} is incorrect. " +
                        "Roster vector element with index [{1}] refers to instance of roster group {2} by index [{3}]" +
                        "but roster group has only {4} roster instances.",
                        FormatQuestionForException(questionId, questionnaire), indexOfRosterVectorElement,
                        FormatGroupForException(rosterGroupId, questionnaire), rosterGroupInstanceIndex, countOfRosterGroupInstances));
            }
        }

        private void ThrowIfQuestionTypeIsNotOneOfExpected(Guid questionId, IQuestionnaire questionnaire, params QuestionType[] expectedQuestionTypes)
        {
            QuestionType questionType = questionnaire.GetQuestionType(questionId);

            bool typeIsNotExpected = !expectedQuestionTypes.Contains(questionType);
            if (typeIsNotExpected)
                throw new InterviewException(string.Format(
                    "Question {0} has type {1}. But one of the following types was expected: {2}.",
                    FormatQuestionForException(questionId, questionnaire), questionType, string.Join(", ", expectedQuestionTypes.Select(type => type.ToString()))));
        } 

        private void ThrowIfNumericQuestionIsNotReal(Guid questionId, IQuestionnaire questionnaire)
        {
            var isNotSupportReal = questionnaire.IsQuestionInteger(questionId);
            if (isNotSupportReal)
                throw new InterviewException(string.Format(
                    "Question {0} doesn't support answer of type real.",
                    FormatQuestionForException(questionId, questionnaire)));
        }

        private void ThrowIfNumericQuestionIsNotInteger(Guid questionId, IQuestionnaire questionnaire)
        {
            var isNotSupportInteger = !questionnaire.IsQuestionInteger(questionId);
            if (isNotSupportInteger)
                throw new InterviewException(string.Format(
                    "Question {0} doesn't support answer of type integer.",
                    FormatQuestionForException(questionId, questionnaire)));
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

        private static void ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(Guid questionId, int answersCount, IQuestionnaire questionnaire)
        {
            int? maxSelectedOptions = questionnaire.GetMaxSelectedAnswerOptions(questionId);

            if (maxSelectedOptions.HasValue && maxSelectedOptions > 0 && answersCount > maxSelectedOptions)
                throw new InterviewException(string.Format(
                    "For question {0} number of answers is greater than the maximum number of selected answers", FormatQuestionForException(questionId, questionnaire)));
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
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(parentGroupIds, question.RosterVector, questionnaire);

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

        private void ThrowIfAnswerHasMoreDecimalPlacesThenAccepted(IQuestionnaire questionnaire, Guid questionId, decimal answer)
        {
            int? countOfDecimalPlacesAllowed = questionnaire.GetCountOfDecimalPlacesAllowedByQuestion(questionId);
            if (!countOfDecimalPlacesAllowed.HasValue)
                return;

            var roundedAnswer = Math.Round(answer, countOfDecimalPlacesAllowed.Value);
            if (roundedAnswer!=answer)
                throw new InterviewException(
                    string.Format(
                        "Answer '{0}' for question {1}  is incorrect because has more decimal places then allowed by questionnaire", answer,
                        FormatQuestionForException(questionId, questionnaire)));
        }

        private static void ThrowIfNumericAnswerExceedsMaxValue(Guid questionId, decimal answer, IQuestionnaire questionnaire)
        {
            int? maxValue = questionnaire.GetMaxValueForNumericQuestion(questionId);

            if (!maxValue.HasValue)
                return;

            bool answerExceedsMaxValue = answer > maxValue.Value;

            if (answerExceedsMaxValue)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question {1} is incorrect because answer is greater than max value '{2}'.",
                    answer, FormatQuestionForException(questionId, questionnaire), maxValue.Value));
        }

        private static void ThrowIfRosterSizeAnswerIsNegative(Guid questionId, int answer, IQuestionnaire questionnaire)
        {
            bool answerIsNegative = answer < 0;

            if (answerIsNegative)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question {1} is incorrect because question is used as size of roster group and specified answer is negative.",
                    answer, FormatQuestionForException(questionId, questionnaire)));
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
                                                                  InterviewStatus.Restored,
                                                                  InterviewStatus.RejectedBySupervisor,
                                                                  InterviewStatus.Restarted);
                    return;
                case InterviewStatus.RejectedBySupervisor:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(
                        InterviewStatus.InterviewerAssigned,
                        InterviewStatus.Restored,
                        InterviewStatus.RejectedBySupervisor,
                        InterviewStatus.Restarted,
                        InterviewStatus.Completed, 
                        InterviewStatus.ApprovedBySupervisor);
                    return;
                case InterviewStatus.InterviewerAssigned:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(
                        InterviewStatus.InterviewerAssigned,
                        InterviewStatus.Restored,
                        InterviewStatus.RejectedBySupervisor,
                        InterviewStatus.SupervisorAssigned,
                        InterviewStatus.Restarted);
                    return;
            }
            throw new InterviewException(string.Format(
                "Status {0} not allowed to be changed with ApplySynchronizationMetadata command",
                interviewStatus));
        }



        private void CalculateRosterData(decimal[] rosterVector, List<Guid> idsOfRosterGroups, int rosterSize, IQuestionnaire questionnaire,
            Func<Identity, object> getAnswer, Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds, out List<Identity> answersToRemoveByDecreasedRosterSize,
            out List<Identity> initializedGroupsToBeDisabled, out List<Identity> initializedGroupsToBeEnabled, out List<Identity> initializedQuestionsToBeDisabled,
            out List<Identity> initializedQuestionsToBeEnabled, out List<Identity> initializedQuestionsToBeInvalid, out List<RosterIdentity> rosterInstancesToAdd,
            out List<RosterIdentity> rosterInstancesToDelete)
        {
            answersToRemoveByDecreasedRosterSize = this.GetAnswersToRemoveIfRosterSizeIsBeingDecreased(
                idsOfRosterGroups, rosterSize, rosterVector, questionnaire);

            this.DetermineCustomEnablementStateOfGroupsInitializedByIncreasedRosterSize(
                idsOfRosterGroups, rosterSize, rosterVector, questionnaire, getAnswer, getRosterInstanceIds,
                out initializedGroupsToBeDisabled, out initializedGroupsToBeEnabled);
            this.DetermineCustomEnablementStateOfQuestionsInitializedByIncreasedRosterSize(
                idsOfRosterGroups, rosterSize, rosterVector, questionnaire, getAnswer, getRosterInstanceIds,
                out initializedQuestionsToBeDisabled, out initializedQuestionsToBeEnabled);
            this.DetermineValidityStateOfQuestionsInitializedByIncreasedRosterSize(
                idsOfRosterGroups, rosterSize, rosterVector, questionnaire, getRosterInstanceIds, initializedGroupsToBeDisabled,
                initializedQuestionsToBeDisabled, out initializedQuestionsToBeInvalid);

            this.PerformRosterSizeChange(rosterVector, idsOfRosterGroups, rosterSize,
                out rosterInstancesToAdd, out rosterInstancesToDelete);
        }

        private void PerformRosterSizeChange(decimal[] rosterVector, List<Guid> idsOfRosterGroups, int rosterSize, out List<RosterIdentity> rosterInstancesToAdd,
            out List<RosterIdentity> rosterInstancesToDelete)
        {
            Dictionary<Guid, List<int>> rosterRowIdsForDelete, rosterRowIdsForAdd;
            this.PerformRosterSizeChange(idsOfRosterGroups, rosterVector, rosterSize, out rosterRowIdsForDelete, out rosterRowIdsForAdd);

            // nested rosters bug: rosterVector cannot be used as outerRosterVector when answered question has roster level 0 but roster has roster level 2

            rosterInstancesToAdd = rosterRowIdsForAdd
                .SelectMany(pair => pair.Value.Select(rosterInstanceId => new RosterIdentity(pair.Key, rosterVector, rosterInstanceId, sortIndex: null)))
                .ToList();

            rosterInstancesToDelete = rosterRowIdsForDelete
                .SelectMany(pair => pair.Value.Select(rosterInstanceId => new RosterIdentity(pair.Key, rosterVector, rosterInstanceId)))
                .ToList();
        }

        private void PerformRosterSizeChange(List<Guid> idsOfRosterGroups, decimal[] rosterVector, int rosterSize,
            out Dictionary<Guid, List<int>> rosterRowIdsForDelete, out Dictionary<Guid, List<int>> rosterRowIdsForAdd)
        {
            rosterRowIdsForAdd = new Dictionary<Guid, List<int>>();
            rosterRowIdsForDelete = new Dictionary<Guid, List<int>>();

            foreach (var idsOfRosterGroup in idsOfRosterGroups)
            {
                var rosterKey = ConvertIdAndRosterVectorToString(idsOfRosterGroup, rosterVector);
                var roster = rosterGroupInstanceCounts.ContainsKey(rosterKey)
                    ? rosterGroupInstanceCounts[rosterKey]
                    : new HashSet<decimal>();

                if (roster.Count == rosterSize)
                    continue;

                if (roster.Count < rosterSize)
                {
                    rosterRowIdsForAdd[idsOfRosterGroup] = new List<int>();
                    for (int i = roster.Count; i < rosterSize; i++)
                    {
                        rosterRowIdsForAdd[idsOfRosterGroup].Add(i);
                    }
                }
                else
                {
                    rosterRowIdsForDelete[idsOfRosterGroup] = new List<int>();
                    for (int i = rosterSize; i < roster.Count; i++)
                    {
                        rosterRowIdsForDelete[idsOfRosterGroup].Add(i);
                    }
                }
            }
        }


        private void PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(
            Identity answeredQuestion, IQuestionnaire questionnaire, Func<Identity, object> getAnswer, Func<Identity, bool?> getNewQuestionStatus,
            List<Identity> groupsToBeEnabled, List<Identity> questionsToBeEnabled,
            out List<Identity> questionsToBeDeclaredValid, out List<Identity> questionsToBeDeclaredInvalid)
        {
            questionsToBeDeclaredValid = new List<Identity>();
            questionsToBeDeclaredInvalid = new List<Identity>();

            bool? answeredQuestionValidationResult = this.PerformValidationOfQuestion(answeredQuestion, questionnaire, getAnswer, getNewQuestionStatus);
            switch (answeredQuestionValidationResult)
            {
                case true: questionsToBeDeclaredValid.Add(answeredQuestion); break;
                case false: questionsToBeDeclaredInvalid.Add(answeredQuestion); break;
            }

            List<Identity> dependentQuestionsDeclaredValid;
            List<Identity> dependentQuestionsDeclaredInvalid;
            this.PerformValidationOfDependentQuestionsAndJustEnabledQuestions(answeredQuestion, questionnaire, getAnswer, this.GetRosterInstanceIds, getNewQuestionStatus,
                groupsToBeEnabled,questionsToBeEnabled,
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

        private void PerformValidationOfDependentQuestionsAndJustEnabledQuestions(Identity question, IQuestionnaire questionnaire,
            Func<Identity, object> getAnswer, Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds, Func<Identity, bool?> getNewQuestionStatus,
            List<Identity> groupsToBeEnabled, List<Identity> questionsToBeEnabled,
            out List<Identity> questionsDeclaredValid, out List<Identity> questionsDeclaredInvalid)
        {
            questionsDeclaredValid = new List<Identity>();
            questionsDeclaredInvalid = new List<Identity>();

            IEnumerable<Guid> dependentQuestionIds = questionnaire.GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(question.Id);
            IEnumerable<Identity> dependentQuestions = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                dependentQuestionIds, question.RosterVector, questionnaire, getRosterInstanceIds);

            IEnumerable<Identity> mandatoryQuestionsAndQuestionsWithCustomValidationFromJustEnabledGroupsAndQuestions =
                this.GetMandatoryQuestionsAndQuestionsWithCustomValidationFromJustEnabledGroupsAndQuestions(
                    questionnaire, groupsToBeEnabled, questionsToBeEnabled, getRosterInstanceIds);

            var dependendQuestionsAndJustEnabled = dependentQuestions.Union(mandatoryQuestionsAndQuestionsWithCustomValidationFromJustEnabledGroupsAndQuestions).ToList();

            foreach (Identity dependentQuestion in dependendQuestionsAndJustEnabled)
            {
                bool? dependentQuestionValidationResult = this.PerformValidationOfQuestion(dependentQuestion, questionnaire, getAnswer, getNewQuestionStatus);
                switch (dependentQuestionValidationResult)
                {
                    case true: questionsDeclaredValid.Add(dependentQuestion); break;
                    case false: questionsDeclaredInvalid.Add(dependentQuestion); break;
                }
            }
        }

        private IEnumerable<Identity> GetMandatoryQuestionsAndQuestionsWithCustomValidationFromJustEnabledGroupsAndQuestions(IQuestionnaire questionnaire,
            List<Identity> groupsToBeEnabled, List<Identity> questionsToBeEnabled,
            Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds)
        {
            foreach (var question in questionsToBeEnabled)
            {
                if (questionnaire.IsQuestionMandatory(question.Id) || questionnaire.IsCustomValidationDefined(question.Id))
                    yield return question;
            }

            foreach (var group in groupsToBeEnabled)
            {
                IEnumerable<Guid> affectedUnderlyingQuestionIds = 
                    questionnaire.GetUnderlyingMandatoryQuestions(group.Id)
                    .Union(questionnaire.GetUnderlyingQuestionsWithNotEmptyCustomValidationExpressions(group.Id));

                IEnumerable<Identity> affectedUnderlyingQuestionInstances = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                    affectedUnderlyingQuestionIds, group.RosterVector, questionnaire, getRosterInstanceIds);

                foreach (var underlyingQuestionInstance in affectedUnderlyingQuestionInstances)
                {
                    yield return underlyingQuestionInstance;
                }
            }
        }

        private bool? PerformValidationOfQuestion(Identity question, IQuestionnaire questionnaire, Func<Identity, object> getAnswer, Func<Identity, bool?> getNewQuestionState)
        {
            if (questionnaire.IsQuestionMandatory(question.Id))
            {
                if (getAnswer(question) == null)
                    return false;
            }

            if (!questionnaire.IsCustomValidationDefined(question.Id))
                return true;
            
            if (getAnswer(question) == null)
                return true;

            bool? questionChangedState = getNewQuestionState(question);

            //we treat newly disabled questions with validations as valid
            if (questionChangedState == false)
                return true;

            if (questionChangedState == null && this.IsQuestionDisabled(question))
                return true;

            string validationExpression = questionnaire.GetCustomValidationExpression(question.Id);

            IEnumerable<QuestionIdAndVariableName> involvedQuestionIds = questionnaire.GetQuestionsInvolvedInCustomValidation(question.Id);
            IEnumerable<KeyValuePair<string,Identity>> involvedQuestions = GetInstancesOfQuestionsWithSameAndUpperRosterLevelOrThrow(involvedQuestionIds, question.RosterVector, questionnaire);

            return this.EvaluateBooleanExpressionOrReturnNullIfExecutionFailsWhenNotEnoughAnswers(
                validationExpression, involvedQuestions, getAnswer, resultIfExecutionFailsWhenAnswersAreEnough: false,
                thisIdentifierQuestionId: question.Id);
        }


        private void DetermineCustomEnablementStateOfDependentGroups(Identity question, IQuestionnaire questionnaire,
            Func<Identity, object> getAnswer, Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds,
            out List<Identity> groupsToBeDisabled, out List<Identity> groupsToBeEnabled)
        {
            groupsToBeDisabled = new List<Identity>();
            groupsToBeEnabled = new List<Identity>();

            IEnumerable<Guid> dependentGroupIds = questionnaire.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(question.Id);
            IEnumerable<Identity> dependentGroups = GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(
                dependentGroupIds, question.RosterVector, questionnaire, getRosterInstanceIds);

            foreach (Identity dependentGroup in dependentGroups)
            {
                PutToCorrespondingListAccordingToEnablementStateChange(dependentGroup, groupsToBeEnabled, groupsToBeDisabled,
                    isNewStateEnabled: this.ShouldGroupBeEnabledByCustomEnablementCondition(dependentGroup, questionnaire, getAnswer),
                    isOldStateEnabled: !this.IsGroupDisabled(dependentGroup));
            }
        }

        private void DetermineCustomEnablementStateOfDependentQuestions(Identity questionBeingAnswered, object answerBeingApplied,
            IQuestionnaire questionnaire, Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds,
            out List<Identity> questionsToBeDisabled, out List<Identity> questionsToBeEnabled)
        {
            var collectedQuestionsToBeDisabled = new List<Identity>();
            var collectedQuestionsToBeEnabled = new List<Identity>();

            Func<Identity, bool> isQuestionDisabled = question =>
            {
                bool isQuestionToBeDisabled = collectedQuestionsToBeDisabled.Any(questionToBeDisabled => AreEqual(questionToBeDisabled, question));
                bool isQuestionToBeEnabled = collectedQuestionsToBeEnabled.Any(questionToBeEnabled => AreEqual(questionToBeEnabled, question));
                
                return isQuestionToBeDisabled || !isQuestionToBeEnabled && this.IsQuestionDisabled(question);
            };

            Func<Identity, object> getAnswer = question =>
                this.GetEnabledQuestionAnswerSupportedInExpressions(question, isQuestionDisabled, questionBeingAnswered, answerBeingApplied);

            var processedQuestionKeys = new HashSet<string> { ConvertIdAndRosterVectorToString(questionBeingAnswered.Id, questionBeingAnswered.RosterVector) };
            var affectingQuestions = new Queue<Identity>(new [] { questionBeingAnswered });

            while (affectingQuestions.Count > 0)
            {
                Identity affectingQuestion = affectingQuestions.Dequeue();

                IEnumerable<Guid> dependentQuestionIds = questionnaire.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(
                    affectingQuestion.Id);
                IEnumerable<Identity> dependentQuestions = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                    dependentQuestionIds, affectingQuestion.RosterVector, questionnaire, getRosterInstanceIds);

                foreach (Identity dependentQuestion in dependentQuestions)
                {
                    PutToCorrespondingListAccordingToEnablementStateChange(dependentQuestion,
                        collectedQuestionsToBeEnabled, collectedQuestionsToBeDisabled,
                        isNewStateEnabled: this.ShouldQuestionBeEnabledByCustomEnablementCondition(dependentQuestion, questionnaire, getAnswer),
                        isOldStateEnabled: !this.IsQuestionDisabled(dependentQuestion));

                    processedQuestionKeys.Add(ConvertIdAndRosterVectorToString(dependentQuestion.Id, dependentQuestion.RosterVector));

                    if (this.ShouldQuestionBeEnabledByCustomEnablementCondition(dependentQuestion, questionnaire, getAnswer) != !this.IsQuestionDisabled(dependentQuestion))
                        affectingQuestions.Enqueue(dependentQuestion);
                }
            }

            questionsToBeDisabled = collectedQuestionsToBeDisabled;
            questionsToBeEnabled = collectedQuestionsToBeEnabled;
        }

        private void DetermineCustomEnablementStateOfGroupsInitializedByIncreasedRosterSize(
            IEnumerable<Guid> idsOfGroupsBeingRostered, int rosterSize, decimal[] outerScopeRosterVector, IQuestionnaire questionnaire,
            Func<Identity, object> getAnswer, Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds,
            out List<Identity> groupsToBeDisabled, out List<Identity> groupsToBeEnabled)
        {
            groupsToBeDisabled = new List<Identity>();
            groupsToBeEnabled = new List<Identity>();

            foreach (Guid idOfGroupBeingRostered in idsOfGroupsBeingRostered)
            {
                int oldRosterSize = this.GetCountOfRosterGroupInstances(idOfGroupBeingRostered, outerScopeRosterVector);

                bool isRosterSizeBeingIncreased = rosterSize > oldRosterSize;
                if (!isRosterSizeBeingIncreased)
                    continue;

                int indexOfGroupBeingRosteredInRosterVector = GetIndexOfRosterGroupInRosterVector(idOfGroupBeingRostered, questionnaire);

                IEnumerable<Guid> affectedGroupIds = questionnaire.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(idOfGroupBeingRostered);

                IEnumerable<Identity> affectedGroups = GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(
                        affectedGroupIds, outerScopeRosterVector, questionnaire, getRosterInstanceIds)
                    .Where(group => IsInstanceBeingInitializedByIncreasedRosterSize(group.RosterVector, oldRosterSize, indexOfGroupBeingRosteredInRosterVector));

                foreach (Identity group in affectedGroups)
                {
                    PutToCorrespondingListAccordingToEnablementStateChange(group, groupsToBeEnabled, groupsToBeDisabled,
                        isNewStateEnabled: this.ShouldGroupBeEnabledByCustomEnablementCondition(group, questionnaire, getAnswer),
                        isOldStateEnabled: true);
                }
            }
        }

        private void DetermineCustomEnablementStateOfQuestionsInitializedByIncreasedRosterSize(
            IEnumerable<Guid> idsOfGroupsBeingRostered, int rosterSize, decimal[] outerScopeRosterVector, IQuestionnaire questionnaire,
            Func<Identity, object> getAnswer, Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds,
            out List<Identity> questionsToBeDisabled, out List<Identity> questionsToBeEnabled)
        {
            questionsToBeDisabled = new List<Identity>();
            questionsToBeEnabled = new List<Identity>();

            foreach (Guid idOfGroupBeingRostered in idsOfGroupsBeingRostered)
            {
                int oldRosterSize = this.GetCountOfRosterGroupInstances(idOfGroupBeingRostered, outerScopeRosterVector);

                bool isRosterSizeBeingIncreased = rosterSize > oldRosterSize;
                if (!isRosterSizeBeingIncreased)
                    continue;

                int indexOfGroupBeingRosteredInRosterVector = GetIndexOfRosterGroupInRosterVector(idOfGroupBeingRostered, questionnaire);

                IEnumerable<Guid> affectedQuestionIds = questionnaire.GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(idOfGroupBeingRostered);

                IEnumerable<Identity> affectedQuestions = this
                    .GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                        affectedQuestionIds, outerScopeRosterVector, questionnaire, getRosterInstanceIds)
                    .Where(group => IsInstanceBeingInitializedByIncreasedRosterSize(group.RosterVector, oldRosterSize, indexOfGroupBeingRosteredInRosterVector));

                foreach (Identity question in affectedQuestions)
                {
                    PutToCorrespondingListAccordingToEnablementStateChange(question, questionsToBeEnabled, questionsToBeDisabled,
                        isNewStateEnabled: this.ShouldQuestionBeEnabledByCustomEnablementCondition(question, questionnaire, getAnswer),
                        isOldStateEnabled: true);
                }
            }
        }

        private void DetermineValidityStateOfQuestionsInitializedByIncreasedRosterSize(List<Guid> idsOfGroupsToBeRostered,
            int rosterSize, decimal[] outerScopeRosterVector, IQuestionnaire questionnaire,
            Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds, List<Identity> groupsToBeDisabled,
            List<Identity> questionsToBeDisabled, out List<Identity> questionsToBeInvalid)
        {
            questionsToBeInvalid = new List<Identity>();

            foreach (Guid idOfGroupBeingRostered in idsOfGroupsToBeRostered)
            {
                int oldRosterSize = this.GetCountOfRosterGroupInstances(idOfGroupBeingRostered, outerScopeRosterVector);

                bool isRosterSizeBeingIncreased = rosterSize > oldRosterSize;
                if (!isRosterSizeBeingIncreased)
                    continue;

                int indexOfGroupBeingRosteredInRosterVector = GetIndexOfRosterGroupInRosterVector(idOfGroupBeingRostered,
                    questionnaire);

                IEnumerable<Guid> affectedQuestionIds = questionnaire.GetUnderlyingMandatoryQuestions(idOfGroupBeingRostered);

                IEnumerable<Identity> affectedQuestions = this
                    .GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                        affectedQuestionIds, outerScopeRosterVector, questionnaire, getRosterInstanceIds)
                    .Where(
                        question =>
                            IsInstanceBeingInitializedByIncreasedRosterSize(question.RosterVector, oldRosterSize,
                                indexOfGroupBeingRosteredInRosterVector) &&
                                !IsQuestionOrParentGroupDisabled(question, questionnaire, (questionId) => groupsToBeDisabled.Any(q => AreEqual(q, questionId)), (questionId) => questionsToBeDisabled.Any(q => AreEqual(q, questionId))));

                questionsToBeInvalid.AddRange(affectedQuestions);
            }
        }

        private bool ShouldGroupBeEnabledByCustomEnablementCondition(Identity group, IQuestionnaire questionnaire, Func<Identity, object> getAnswer)
        {
            return this.ShouldBeEnabledByCustomEnablementCondition(
                questionnaire.GetCustomEnablementConditionForGroup(group.Id),
                group.RosterVector,
                questionnaire.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(group.Id),
                questionnaire,
                getAnswer);
        }

        private bool ShouldQuestionBeEnabledByCustomEnablementCondition(Identity question, IQuestionnaire questionnaire, Func<Identity, object> getAnswer)
        {
            return this.ShouldBeEnabledByCustomEnablementCondition(
                questionnaire.GetCustomEnablementConditionForQuestion(question.Id),
                question.RosterVector,
                questionnaire.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(question.Id),
                questionnaire,
                getAnswer);
        }

        private bool ShouldBeEnabledByCustomEnablementCondition(string enablementCondition, decimal[] rosterVector, IEnumerable<QuestionIdAndVariableName> involvedQuestionIds, IQuestionnaire questionnaire, Func<Identity, object> getAnswer)
        {
            const bool ShouldBeEnabledIfSomeInvolvedQuestionsAreNotAnswered = false;

            IEnumerable<KeyValuePair<string, Identity>> involvedQuestions = GetInstancesOfQuestionsWithSameAndUpperRosterLevelOrThrow(involvedQuestionIds, rosterVector, questionnaire);

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

        private static IEnumerable<KeyValuePair<string, Identity>> GetInstancesOfQuestionsWithSameAndUpperRosterLevelOrThrow(
            IEnumerable<QuestionIdAndVariableName> questionIds, decimal[] rosterVector, IQuestionnaire questionnare)
        {
            return questionIds.Select(
                questionId => GetInstanceOfQuestionWithSameAndUpperRosterLevelOrThrow(questionId, rosterVector, questionnare));
        }

        private static KeyValuePair<string, Identity> GetInstanceOfQuestionWithSameAndUpperRosterLevelOrThrow(QuestionIdAndVariableName questionId, decimal[] rosterVector, IQuestionnaire questionnare)
        {
            int vectorRosterLevel = rosterVector.Length;
            int questionRosterLevel = questionnare.GetRosterLevelForQuestion(questionId.Id);

            if (questionRosterLevel > vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Question {0} expected to have roster level not deeper than {1} but it is {2}.",
                    FormatQuestionForException(questionId.Id, questionnare), vectorRosterLevel, questionRosterLevel));

            decimal[] questionRosterVector = ShrinkRosterVector(rosterVector, questionRosterLevel);

            return new KeyValuePair<string, Identity>(questionId.VariableName, new Identity(questionId.Id, questionRosterVector));
        }

        private IEnumerable<Identity> GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
            IEnumerable<Guid> questionIds, decimal[] rosterVector, IQuestionnaire questionnare,
            Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds)
        {
            return questionIds.SelectMany(questionId =>
                this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(questionId, rosterVector, questionnare, getRosterInstanceIds));
        }

        private IEnumerable<Identity> GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
            Guid questionId, decimal[] rosterVector, IQuestionnaire questionnare,
            Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds)
        {
            int vectorRosterLevel = rosterVector.Length;
            int questionRosterLevel = questionnare.GetRosterLevelForQuestion(questionId);

            if (questionRosterLevel < vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Question {0} expected to have roster level not upper than {1} but it is {2}.",
                    FormatQuestionForException(questionId, questionnare), vectorRosterLevel, questionRosterLevel));

            Guid[] parentRosterGroupsStartingFromTop =
                questionnare.GetParentRosterGroupsForQuestionStartingFromTop(questionId).ToArray();

            IEnumerable<decimal[]> questionRosterVectors = ExtendRosterVector(
                rosterVector, questionRosterLevel, parentRosterGroupsStartingFromTop, getRosterInstanceIds);

            foreach (decimal[] questionRosterVector in questionRosterVectors)
            {
                yield return new Identity(questionId, questionRosterVector);
            }
        }

        private static IEnumerable<Identity> GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(
            IEnumerable<Guid> groupIds, decimal[] rosterVector, IQuestionnaire questionnare)
        {
            int vectorRosterLevel = rosterVector.Length;

            foreach (Guid groupId in groupIds)
            {
                int groupRosterLevel = questionnare.GetRosterLevelForGroup(groupId);

                if (groupRosterLevel > vectorRosterLevel)
                    throw new InterviewException(string.Format(
                        "Group {0} expected to have roster level not deeper than {1} but it is {2}.",
                        FormatGroupForException(groupId, questionnare), vectorRosterLevel, groupRosterLevel));

                decimal[] groupRosterVector = ShrinkRosterVector(rosterVector, groupRosterLevel);

                yield return new Identity(groupId, groupRosterVector);
            }
        }

        private static IEnumerable<Identity> GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(
            IEnumerable<Guid> groupIds, decimal[] rosterVector, IQuestionnaire questionnare,
            Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds)
        {
            int vectorRosterLevel = rosterVector.Length;

            foreach (Guid groupId in groupIds)
            {
                int groupRosterLevel = questionnare.GetRosterLevelForGroup(groupId);

                if (groupRosterLevel < vectorRosterLevel)
                    throw new InterviewException(string.Format(
                        "Group {0} expected to have roster level not upper than {1} but it is {2}.",
                        FormatGroupForException(groupId, questionnare), vectorRosterLevel, groupRosterLevel));

                Guid[] rosterGroupsStartingFromTop = questionnare.GetParentRosterGroupsAndGroupItselfIfRosterStartingFromTop(groupId).ToArray();
                IEnumerable<decimal[]> groupRosterVectors = ExtendRosterVector(
                    rosterVector, groupRosterLevel, rosterGroupsStartingFromTop, getRosterInstanceIds);

                foreach (decimal[] groupRosterVector in groupRosterVectors)
                {
                    yield return new Identity(groupId, groupRosterVector);
                }
            }
        }

        private static List<Identity> GetGroupsToBeDisabledInJustCreatedInterview(IQuestionnaire questionnaire)
        {
            return questionnaire
                .GetAllGroupsWithNotEmptyCustomEnablementConditions()
                .Where(groupId => !questionnaire.IsRosterGroup(groupId))
                .Where(groupId => !IsGroupUnderRosterGroup(questionnaire, groupId))
                .Select(groupId => new Identity(groupId, EmptyRosterVector))
                .ToList();
        }

        private static List<Identity> GetQuestionsToBeDisabledInJustCreatedInterview(IQuestionnaire questionnaire)
        {
            return questionnaire
                .GetAllQuestionsWithNotEmptyCustomEnablementConditions()
                .Where(questionId => !IsQuestionUnderRosterGroup(questionnaire, questionId))
                .Select(questionId => new Identity(questionId, EmptyRosterVector))
                .ToList();
        }

        private List<Identity> GetQuestionsToBeInvalidInJustCreatedInterview(IQuestionnaire questionnaire, List<Identity> groupsToBeDisabled, List<Identity> questionsToBeDisabled)
        {
            return questionnaire
                .GetAllMandatoryQuestions()
                .Where(
                    questionId =>
                        !IsQuestionUnderRosterGroup(questionnaire, questionId) &&
                            !IsQuestionOrParentGroupDisabled(new Identity(questionId, new decimal[0]), questionnaire,
                                (question) => groupsToBeDisabled.Any(q => AreEqual(q, question)),
                                (question) => questionsToBeDisabled.Any(q => AreEqual(q, question))))
                .Select(questionId => new Identity(questionId, EmptyRosterVector))
                .ToList();
        }

        private static bool IsGroupUnderRosterGroup(IQuestionnaire questionnaire, Guid groupId)
        {
            return questionnaire.GetParentRosterGroupsAndGroupItselfIfRosterStartingFromTop(groupId).Any();
        }

        private static bool IsQuestionUnderRosterGroup(IQuestionnaire questionnaire, Guid questionId)
        {
            return questionnaire.GetParentRosterGroupsForQuestionStartingFromTop(questionId).Any();
        }

        private List<Identity> GetAnswersToRemoveIfRosterSizeIsBeingDecreased(
            IEnumerable<Guid> idsOfGroupsBeingRostered, int rosterSize, decimal[] outerScopeRosterVector, IQuestionnaire questionnaire)
        {
            return idsOfGroupsBeingRostered
                .SelectMany(idOfGroupBeingRostered => this.GetAnswersToRemoveIfRosterSizeIsBeingDecreased(idOfGroupBeingRostered, rosterSize, outerScopeRosterVector, questionnaire))
                .ToList();
        }

        private IEnumerable<Identity> GetAnswersToRemoveIfRosterSizeIsBeingDecreased(
            Guid idOfGroupBeingRostered, int rosterSize, decimal[] outerScopeRosterVector, IQuestionnaire questionnaire)
        {
            bool isRosterSizeBeingDecreased = rosterSize < this.GetCountOfRosterGroupInstances(idOfGroupBeingRostered, outerScopeRosterVector);
            if (!isRosterSizeBeingDecreased)
                return Enumerable.Empty<Identity>();

            int indexOfGroupBeingRosteredInRosterVector = GetIndexOfRosterGroupInRosterVector(idOfGroupBeingRostered, questionnaire);

            IEnumerable<Guid> underlyingQuestionIds = questionnaire.GetAllUnderlyingQuestions(idOfGroupBeingRostered);

            IEnumerable<Identity> underlyingQuestionInstances = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                underlyingQuestionIds, outerScopeRosterVector, questionnaire, this.GetRosterInstanceIds);

            IEnumerable<Identity> underlyingQuestionsBeingRemovedByDecreasedRosterSize = (
                from question in underlyingQuestionInstances
                where this.WasQuestionAnswered(question)
                where IsInstanceBeingRemovedByDecreasedRosterSize(question.RosterVector, rosterSize, indexOfGroupBeingRosteredInRosterVector)
                select question
            ).ToList();

            IEnumerable<Identity> linkedQuestionsWithNoLongerValidAnswersBecauseOfSelectedOptionBeingRemoved =
                GetAnswersForLinkedQuestionsToRemoveBecauseOfRemovedQuestionAnswers(
                    underlyingQuestionsBeingRemovedByDecreasedRosterSize, questionnaire, this.GetRosterInstanceIds);

            return Enumerable.Concat(
                underlyingQuestionsBeingRemovedByDecreasedRosterSize,
                linkedQuestionsWithNoLongerValidAnswersBecauseOfSelectedOptionBeingRemoved);
        }

        private static bool IsInstanceBeingInitializedByIncreasedRosterSize(decimal[] instanceRosterVector, int oldRosterSize, int indexOfGroupBeingRosteredInRosterVector)
        {
            return instanceRosterVector[indexOfGroupBeingRosteredInRosterVector] >= oldRosterSize;
        }

        private static bool IsInstanceBeingRemovedByDecreasedRosterSize(decimal[] instanceRosterVector, int newRosterSize, int indexOfGroupBeingRosteredInRosterVector)
        {
            return instanceRosterVector[indexOfGroupBeingRosteredInRosterVector] >= newRosterSize;
        }

        private IEnumerable<Identity> GetAnswersForLinkedQuestionsToRemoveBecauseOfRemovedQuestionAnswers(
            IEnumerable<Identity> questionsToRemove, IQuestionnaire questionnaire,
            Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds)
        {
            bool nothingGoingToBeRemoved = !questionsToRemove.Any();
            if (nothingGoingToBeRemoved)
                return Enumerable.Empty<Identity>();

            return this.GetAnswersForLinkedQuestionsToRemoveBecauseOfReferencedAnswersGoingToDisappear(questionnaire, getRosterInstanceIds,
                isQuestionAnswerGoingToDisappear: question => questionsToRemove.Any(questionToRemove => AreEqual(question, questionToRemove)));
        }

        private List<Identity> GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
            IEnumerable<Identity> groupsToBeDisabled, IEnumerable<Identity> questionsToBeDisabled, IQuestionnaire questionnaire,
            Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds)
        {
            bool nothingGoingToBeDisabled = !groupsToBeDisabled.Any() && !questionsToBeDisabled.Any();
            if (nothingGoingToBeDisabled)
                return new List<Identity>();

            return this.GetAnswersForLinkedQuestionsToRemoveBecauseOfReferencedAnswersGoingToDisappear(questionnaire, getRosterInstanceIds,
                isQuestionAnswerGoingToDisappear: question => IsQuestionGoingToBeDisabled(question, groupsToBeDisabled, questionsToBeDisabled, questionnaire));
        }

        private List<Identity> GetAnswersForLinkedQuestionsToRemoveBecauseOfReferencedAnswersGoingToDisappear(IQuestionnaire questionnaire,
            Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds, Func<Identity, bool> isQuestionAnswerGoingToDisappear)
        {
            var answersToRemove = new List<Identity>();

            foreach (Tuple<Guid, decimal[], decimal[]> linkedSingleOptionAnswer in this.linkedSingleOptionAnswers.Values)
            {
                var linkedQuestion = new Identity(linkedSingleOptionAnswer.Item1, linkedSingleOptionAnswer.Item2);
                decimal[] linkedQuestionSelectedOption = linkedSingleOptionAnswer.Item3;

                IEnumerable<Identity> questionsReferencedByLinkedQuestion =
                    this.GetQuestionsReferencedByLinkedQuestion(linkedQuestion, questionnaire, getRosterInstanceIds);

                Identity questionSelectedAsAnswer =
                    questionsReferencedByLinkedQuestion
                        .SingleOrDefault(
                            question => AreEqualRosterVectors(linkedQuestionSelectedOption, question.RosterVector));

                bool isSelectedOptionGoingToDisappear = questionSelectedAsAnswer != null && isQuestionAnswerGoingToDisappear(questionSelectedAsAnswer);
                if (isSelectedOptionGoingToDisappear)
                {
                    answersToRemove.Add(linkedQuestion);
                }
            }

            foreach (Tuple<Guid, decimal[], decimal[][]> linkedMultipleOptionsAnswer in this.linkedMultipleOptionsAnswers.Values)
            {
                var linkedQuestion = new Identity(linkedMultipleOptionsAnswer.Item1, linkedMultipleOptionsAnswer.Item2);
                decimal[][] linkedQuestionSelectedOptions = linkedMultipleOptionsAnswer.Item3;

                IEnumerable<Identity> questionsReferencedByLinkedQuestion =
                    this.GetQuestionsReferencedByLinkedQuestion(linkedQuestion, questionnaire, getRosterInstanceIds);

                IEnumerable<Identity> questionsSelectedAsAnswers =
                    questionsReferencedByLinkedQuestion
                        .Where(
                            question => linkedQuestionSelectedOptions.Any(
                                selectedOption => AreEqualRosterVectors(selectedOption, question.RosterVector)));

                bool isSomeOfSelectedOptionsGoingToDisappear = questionsSelectedAsAnswers.Any(isQuestionAnswerGoingToDisappear);
                if (isSomeOfSelectedOptionsGoingToDisappear)
                {
                    answersToRemove.Add(linkedQuestion);
                }
            }

            return answersToRemove;
        }

        private IEnumerable<Identity> GetQuestionsReferencedByLinkedQuestion(
            Identity linkedQuestion, IQuestionnaire questionnaire, Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds)
        {
            Guid referencedQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(linkedQuestion.Id);

            return this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                referencedQuestionId, linkedQuestion.RosterVector, questionnaire, getRosterInstanceIds);
        }

        private static bool IsQuestionGoingToBeDisabled(Identity question,
            IEnumerable<Identity> groupsToBeDisabled, IEnumerable<Identity> questionsToBeDisabled, IQuestionnaire questionnaire)
        {
            bool questionIsListedToBeDisabled =
                questionsToBeDisabled.Any(questionToBeDisabled => AreEqual(question, questionToBeDisabled));

            IEnumerable<Guid> parentGroupIds = questionnaire.GetAllParentGroupsForQuestion(question.Id);
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(parentGroupIds, question.RosterVector, questionnaire);

            bool someOfQuestionParentGroupsAreListedToBeDisabled = parentGroups.Any(parentGroup =>
                groupsToBeDisabled.Any(groupToBeDisabled => AreEqual(parentGroup, groupToBeDisabled)));

            return questionIsListedToBeDisabled || someOfQuestionParentGroupsAreListedToBeDisabled;
        }

        private static int GetIndexOfRosterGroupInRosterVector(Guid rosterGroup, IQuestionnaire questionnaire)
        {
            return questionnaire
                .GetParentRosterGroupsAndGroupItselfIfRosterStartingFromTop(rosterGroup)
                .ToList()
                .IndexOf(rosterGroup);
        }


        private bool? EvaluateBooleanExpressionOrReturnNullIfExecutionFailsWhenNotEnoughAnswers(string expression, IEnumerable<KeyValuePair<string, Identity>> involvedQuestions,
            Func<Identity, object> getAnswer, bool? resultIfExecutionFailsWhenAnswersAreEnough, Guid? thisIdentifierQuestionId = null)
        {
            Dictionary<Guid, object> involvedAnswers = involvedQuestions.ToDictionary(
                involvedQuestion => involvedQuestion.Value.Id,
                involvedQuestion => getAnswer(involvedQuestion.Value));

            Dictionary<string, Guid> questionMappedOnVariableNames = involvedQuestions.ToDictionary(
                involvedQuestion => involvedQuestion.Key,
                involvedQuestion => involvedQuestion.Value.Id);

            bool isSpecialThisIdentifierSupportedByExpression = thisIdentifierQuestionId.HasValue;

            var mapIdentifierToQuestionId = isSpecialThisIdentifierSupportedByExpression
                ? (Func<string, Guid>)(identifier => GetQuestionIdByExpressionIdentifierIncludingThis(identifier, questionMappedOnVariableNames, thisIdentifierQuestionId.Value))
                : (Func<string, Guid>)(identifier => GetQuestionIdByExpressionIdentifierExcludingThis(identifier, questionMappedOnVariableNames));

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

        private static Guid GetQuestionIdByExpressionIdentifierIncludingThis(string identifier,Dictionary<string, Guid> questionMappedOnVariableNames, Guid contextQuestionId)
        {
            if (identifier.ToLower() == "this")
                return contextQuestionId;

            return GetQuestionIdByExpressionIdentifierExcludingThis(identifier, questionMappedOnVariableNames);
        }

        private static Guid GetQuestionIdByExpressionIdentifierExcludingThis(string identifier, Dictionary<string, Guid> questionMappedOnVariableNames)
        {
            Guid questionId;
            if (Guid.TryParse(identifier, out questionId))
                return questionId;
            return questionMappedOnVariableNames[identifier];
        }

        private HashSet<decimal> GetRosterInstanceIds(Guid groupId, decimal[] outerRosterVector)
        {
            return GetRosterInstanceIds(groupId, outerRosterVector, this.GetCountOfRosterGroupInstances);
        }

        private static HashSet<decimal> GetRosterInstanceIds(Guid groupId, decimal[] outerRosterVector,
            Func<Guid, decimal[], int> getCountOfRosterGroupInstances)
        {
            return new HashSet<decimal>(
                Enumerable
                    .Range(0, getCountOfRosterGroupInstances(groupId, outerRosterVector))
                    .Select(rosterInstanceIndex => (decimal)rosterInstanceIndex));
        }

        private int GetCountOfRosterGroupInstances(Guid rosterGroupId, decimal[] outerScopeRosterVector)
        {
            string rosterGroupKey = ConvertIdAndRosterVectorToString(rosterGroupId, outerScopeRosterVector);

            return this.rosterGroupInstanceCounts.ContainsKey(rosterGroupKey)
                ? this.rosterGroupInstanceCounts[rosterGroupKey].Count
                : 0;
        }

        private IEnumerable<decimal[]> AvailableRosterLevelsForGroup(IQuestionnaire questionnaire, Guid groupdId)
        {
            int rosterGroupLevel = questionnaire.GetRosterLevelForGroup(groupdId);

            Guid[] parentRosterGroupsStartingFromTop =
                questionnaire.GetParentRosterGroupsAndGroupItselfIfRosterStartingFromTop(groupdId)
                    .ToArray();

            var availableRosterLevels = ExtendRosterVector(EmptyRosterVector, rosterGroupLevel,
                parentRosterGroupsStartingFromTop, this.GetRosterInstanceIds);
            return availableRosterLevels;
        }

        private IEnumerable<decimal[]> AvailableRosterLevelsForQuestion(IQuestionnaire questionnaire, Guid questionId)
        {
            int questionRosterLevel = questionnaire.GetRosterLevelForQuestion(questionId);

            Guid[] parentRosterGroupsStartingFromTop =
                questionnaire.GetParentRosterGroupsForQuestionStartingFromTop(questionId)
                    .ToArray();

            var availableRosterLevels = ExtendRosterVector(EmptyRosterVector, questionRosterLevel,
                parentRosterGroupsStartingFromTop, this.GetRosterInstanceIds);

            return availableRosterLevels;
        }

        private bool IsQuestionAnsweredValid(Identity question)
        {
            string questionKey = ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return this.validAnsweredQuestions.Contains(questionKey);
        }

        private bool IsQuestionAnsweredInvalid(Identity question)
        {
            string questionKey = ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return this.invalidAnsweredQuestions.Contains(questionKey);
        }

        private bool HasInvalidAnswers()
        {
            return this.invalidAnsweredQuestions.Any();
        }

        private bool IsQuestionOrParentGroupDisabled(Identity question, IQuestionnaire questionnaire, Func<Identity, bool> isGroupDisabled, Func<Identity, bool> isQuestionDisabled)
        {
            if (isQuestionDisabled(question))
                return true;

            IEnumerable<Guid> parentGroupIds = questionnaire.GetAllParentGroupsForQuestion(question.Id);
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(parentGroupIds, question.RosterVector, questionnaire);

            var result = parentGroups.Any(isGroupDisabled);
            return result;
        }

        private bool IsGroupDisabled(Identity group)
        {
            string groupKey = ConvertIdAndRosterVectorToString(group.Id, group.RosterVector);

            return this.disabledGroups.Contains(groupKey);
        }

        private bool IsQuestionDisabled(Identity question)
        {
            string questionKey = ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return this.disabledQuestions.Contains(questionKey);
        }

        private bool WasQuestionAnswered(Identity question)
        {
            string questionKey = ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return this.answeredQuestions.Contains(questionKey);
        }

        private object GetAnswerSupportedInExpressionsForEnabledOrNull(Identity question, Func<Identity, bool?> getNewQuestionState)
        {
            bool? newQuestionState = getNewQuestionState(question);

            //no changes after dis/enable but already marked as disabled
            if (!newQuestionState.HasValue && IsQuestionDisabled(question))
                return null;
            //new state of question is disabled
            if (newQuestionState.HasValue && !newQuestionState.Value)
            {
                return null;
            }
            
            string questionKey = ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return this.answersSupportedInExpressions.ContainsKey(questionKey)
                ? this.answersSupportedInExpressions[questionKey]
                : null;
        }

        private object GetEnabledQuestionAnswerSupportedInExpressions(Identity question)
        {
            return this.GetEnabledQuestionAnswerSupportedInExpressions(question, this.IsQuestionDisabled);
        }

        private object GetEnabledQuestionAnswerSupportedInExpressions(Identity question, Func<Identity, bool> isQuestionDisabled,
            Identity questionBeingAnswered = null, object answerBeingApplied = null)
        {
            if (isQuestionDisabled(question))
                return null;

            if (questionBeingAnswered != null && AreEqual(question, questionBeingAnswered))
                return answerBeingApplied;

            string questionKey = ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return this.answersSupportedInExpressions.ContainsKey(questionKey)
                ? this.answersSupportedInExpressions[questionKey]
                : null;
        }


        private static decimal[] ShrinkRosterVector(decimal[] rosterVector, int length)
        {
            if (length == 0)
                return EmptyRosterVector;

            if (length == rosterVector.Length)
                return rosterVector;

            if (length > rosterVector.Length)
                throw new ArgumentException(string.Format("Cannot shrink vector with length {0} to bigger length {1}.", rosterVector.Length, length));

            return rosterVector.Take(length).ToArray();
        }

        /// <remarks>
        /// If roster vector should be extended, result will be a set of vectors depending on roster count of corresponding groups.
        /// </remarks>
        private static IEnumerable<decimal[]> ExtendRosterVector(decimal[] rosterVector, int length, Guid[] rosterGroupsStartingFromTop,
            Func<Guid, decimal[], HashSet<decimal>> getRosterInstanceIds)
        {
            if (length < rosterVector.Length)
                throw new ArgumentException(string.Format(
                    "Cannot extend vector with length {0} to smaller length {1}.", rosterVector.Length, length));

            if (length == rosterVector.Length)
            {
                yield return rosterVector;
                yield break;
            }

            if (length == rosterVector.Length + 1)
            {
                HashSet<decimal> rosterInstanceIds = getRosterInstanceIds(rosterGroupsStartingFromTop.Last(), rosterVector);

                foreach (decimal rosterInstanceId in rosterInstanceIds)
                {
                    yield return ExtendRosterVectorWithOneValue(rosterVector, rosterInstanceId);
                }
                yield break;
            }

            throw new NotImplementedException(
                "This method does not support roster groups inside roster groups, but may easily support it when needed.");
        }

        private static decimal[] ExtendRosterVectorWithOneValue(decimal[] rosterVector, decimal value)
        {
            return new List<decimal>(rosterVector) { value }.ToArray();
        }

        private static bool AreEqual(Identity identityA, Identity identityB)
        {
            return identityA.Id == identityB.Id
                && AreEqualRosterVectors(identityA.RosterVector, identityB.RosterVector);
        }

        private static bool AreEqualRosterVectors(decimal[] rosterVectorA, decimal[] rosterVectorB)
        {
            return Enumerable.SequenceEqual(rosterVectorA, rosterVectorB);
        }

        private static int ToRosterSize(decimal decimalValue)
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
                string.Join("-", question.RosterVector));
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
                string.Join("-", group.RosterVector));
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

        private static HashSet<string> ToHashSetOfIdAndRosterVectorStrings(IEnumerable<InterviewItemId> synchronizationIdentities)
        {
            return new HashSet<string>(
                synchronizationIdentities.Select(question => ConvertIdAndRosterVectorToString(question.Id, question.PropagationVector)));
        }

        /// <remarks>
        /// The opposite operation (get id or vector from string) should never be performed!
        /// This is one-way transformation. Opposite operation is too slow.
        /// If you need to compactify data and get it back, you should use another datatype, not a string.
        /// </remarks>
        private static string ConvertIdAndRosterVectorToString(Guid id, decimal[] rosterVector)
        {
            return string.Format("{0:N}[{1}]", id, string.Join("-", rosterVector));
        }
    }
}
