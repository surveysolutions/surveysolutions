﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewEventHandlerFunctional : 
        AbstractFunctionalEventHandler<ViewWithSequence<InterviewData>>, 
        ICreateHandler<ViewWithSequence<InterviewData>, InterviewCreated>,
        IUpdateHandler<ViewWithSequence<InterviewData>, InterviewStatusChanged>,
        IUpdateHandler<ViewWithSequence<InterviewData>, SupervisorAssigned>,
        IUpdateHandler<ViewWithSequence<InterviewData>, InterviewerAssigned>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GroupPropagated>,
        IUpdateHandler<ViewWithSequence<InterviewData>, RosterRowAdded>,
        IUpdateHandler<ViewWithSequence<InterviewData>, RosterRowRemoved>,
        IUpdateHandler<ViewWithSequence<InterviewData>, RosterRowTitleChanged>,
        IUpdateHandler<ViewWithSequence<InterviewData>, RosterInstancesAdded>,
        IUpdateHandler<ViewWithSequence<InterviewData>, RosterInstancesRemoved>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswerCommented>,
        IUpdateHandler<ViewWithSequence<InterviewData>, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, NumericRealQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, NumericQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, NumericIntegerQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, TextQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, TextListQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, SingleOptionQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, SingleOptionLinkedQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, DateTimeQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GeoLocationQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, QRBarcodeQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswerRemoved>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswersRemoved>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GroupDisabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GroupEnabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GroupsDisabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GroupsEnabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, QuestionDisabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, QuestionEnabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, QuestionsDisabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, QuestionsEnabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswerDeclaredInvalid>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswerDeclaredValid>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswersDeclaredInvalid>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswersDeclaredValid>,
        IUpdateHandler<ViewWithSequence<InterviewData>, FlagRemovedFromAnswer>,
        IUpdateHandler<ViewWithSequence<InterviewData>, FlagSetToAnswer>,
        IUpdateHandler<ViewWithSequence<InterviewData>, InterviewDeclaredInvalid>,
        IUpdateHandler<ViewWithSequence<InterviewData>, InterviewDeclaredValid>,
        ICreateHandler<ViewWithSequence<InterviewData>, InterviewOnClientCreated>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnriePropagationStructures;
        private readonly ISynchronizationDataStorage syncStorage;

        public override Type[] UsesViews
        {
            get { return new Type[] { typeof(UserDocument), typeof(QuestionnaireRosterStructure) }; }
        }

        public override Type[] BuildsViews
        {
            get { return base.BuildsViews.Union(new[] { typeof (SynchronizationDelta) }).ToArray(); }
        }

        private static string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            if (vector.Length == 0)
                return "#";
            return EventHandlerUtils.CreateLeveKeyFromPropagationVector(vector);
        }

        private RosterScopeDescription GetScopeOfPassedGroup(InterviewData interview, Guid groupId)
        {
            var questionnarie = questionnriePropagationStructures.GetById(interview.QuestionnaireId, interview.QuestionnaireVersion);

            foreach (var scopeId in questionnarie.RosterScopes.Keys)
            {
                if (questionnarie.RosterScopes[scopeId].RosterIdToRosterTitleQuestionIdMap.ContainsKey(groupId))
                {
                    return questionnarie.RosterScopes[scopeId];
                }
            }

            throw new ArgumentException(string.Format("group {0} is missing in any propagation scope of questionnaire",
                                                      groupId));
        }
        
        private void RemoveLevelsFromInterview(InterviewData interview, Dictionary<string, Guid[]> levelKeysForDelete, Guid scopeId)
        {
            foreach (var levelKey in levelKeysForDelete)
            {
                RemoveLevelFromInterview(interview, levelKey.Key, levelKey.Value, scopeId);
            }
        }

        private void AddNewLevelsToInterview(InterviewData interview, int startIndex, int count, decimal[] outerVector, int? sortIndex, RosterScopeDescription scope)
        {
            for (int rosterInstanceId = startIndex; rosterInstanceId < startIndex + count; rosterInstanceId++)
            {
                this.AddLevelToInterview(interview, outerVector, rosterInstanceId, sortIndex, scope);
            }
        }

        private void RemoveLevelFromInterview(InterviewData interview, string levelKey, Guid[] groupIds, Guid scopeId)
        {
            if (interview.Levels.ContainsKey(levelKey))
            {
                var level = interview.Levels[levelKey];

                foreach (var groupId in groupIds)
                {
                    level.DisabledGroups.Remove(groupId);
                }

                if (!level.ScopeIds.ContainsKey(scopeId))
                    return;

                if (level.ScopeIds.Count == 1)
                    interview.Levels.Remove(levelKey);
                else
                {
                    level.ScopeIds.Remove(scopeId);
                }
            }
        }

        private void AddLevelToInterview(InterviewData interview, decimal[] vector, decimal rosterInstanceId, int? sortIndex, RosterScopeDescription scope)
        {
            var newVector = CreateNewVector(vector, rosterInstanceId);
            var levelKey = CreateLevelIdFromPropagationVector(newVector);
            if (!interview.Levels.ContainsKey(levelKey))
                interview.Levels[levelKey] = new InterviewLevel(scope.ScopeId, sortIndex, newVector);
            else
                interview.Levels[levelKey].ScopeIds[scope.ScopeId] = sortIndex;

            var level = interview.Levels[levelKey];
            foreach (var rosterGroupsWithTitleQuestionPair in scope.RosterIdToRosterTitleQuestionIdMap)
            {
                if (rosterGroupsWithTitleQuestionPair.Value != null)
                {
                    if (!level.RosterTitleQuestionIdToRosterIdMap.ContainsKey(
                        rosterGroupsWithTitleQuestionPair.Value.QuestionId))
                    {
                        level.RosterTitleQuestionIdToRosterIdMap.Add(
                            rosterGroupsWithTitleQuestionPair.Value.QuestionId, new List<Guid>());
                    }

                    level.RosterTitleQuestionIdToRosterIdMap[rosterGroupsWithTitleQuestionPair.Value.QuestionId].Add(rosterGroupsWithTitleQuestionPair.Key);

                    level.RosterTitleQuestionDescriptions[rosterGroupsWithTitleQuestionPair.Value.QuestionId] =
                        rosterGroupsWithTitleQuestionPair.Value;
                }
            }
        }

        private decimal[] CreateNewVector(decimal[] outerScopePropagationVector, decimal indexInScope)
        {
            var scopeVecor = new decimal[outerScopePropagationVector.Length + 1];
            outerScopePropagationVector.CopyTo(scopeVecor, 0);
            scopeVecor[scopeVecor.Length - 1] = indexInScope;
            return scopeVecor;
        }

        private List<string> GetLevelsByScopeFromInterview(InterviewData interview, Guid scopeId)
        {
            return interview.Levels.Where(level => level.Value.ScopeIds.ContainsKey(scopeId))
                            .Select(level => level.Key).ToList();
        }

        private static InterviewData PreformActionOnLevel(InterviewData interview, decimal[] vector, Action<InterviewLevel> action)
        {
            var levelId = CreateLevelIdFromPropagationVector(vector);

            if (!interview.Levels.ContainsKey(levelId))
                return interview;

            action(interview.Levels[levelId]);
            return interview;
        }

        private static InterviewData UpdateQuestion(InterviewData interview, decimal[] vector, Guid questionId, Action<InterviewQuestion> update)
        {
            return PreformActionOnLevel(interview, vector, (questionsAtTheLevel) =>
            {
                var answeredQuestion = questionsAtTheLevel.GetOrCreateQuestion(questionId);

                update(answeredQuestion);
            });
        }



        private static InterviewData ChangeQuestionConditionState(InterviewData interview, decimal[] vector, Guid questionId, bool newState)
        {
            return UpdateQuestion(interview, vector, questionId, (question) =>
            {
                question.Enabled = newState;
            });
        }

        private static InterviewData ChangeQuestionConditionValidity(InterviewData interview, decimal[] vector, Guid questionId, bool valid)
        {
            return UpdateQuestion(interview, vector, questionId, (question) =>
            {
                question.Valid = valid;
            });
        }

        private InterviewData SaveAnswer<T>(InterviewData interview, decimal[] vector, Guid questionId, T answer)
        {
            return PreformActionOnLevel(interview, vector, (level) =>
            {
                var answeredQuestion = level.GetOrCreateQuestion(questionId);

                answeredQuestion.Answer = answer;
                answeredQuestion.IsAnswered = true;

                if (level.RosterTitleQuestionIdToRosterIdMap.ContainsKey(questionId))
                {
                    var groupIds = level.RosterTitleQuestionIdToRosterIdMap[questionId];

                    var questionDescription = level.RosterTitleQuestionDescriptions.ContainsKey(questionId)
                        ? level.RosterTitleQuestionDescriptions[questionId]
                        : null;

                    var answerString =
                        questionDescription != null && questionDescription.Options.Any()
                            ? AnswerUtils.AnswerToString(answer, value => questionDescription.Options[value])
                            : AnswerUtils.AnswerToString(answer);

                    foreach (var groupId in groupIds)
                    {
                        if (level.RosterRowTitles.ContainsKey(groupId))
                        {
                            level.RosterRowTitles[groupId] = answerString;
                        }
                        else
                        {
                            level.RosterRowTitles.Add(groupId, answerString);
                        }
                    }
                }
            });
        }

        private static InterviewData SetFlagStateForQuestion(InterviewData interview, decimal[] vector, Guid questionId, bool isFlagged)
        {
            return UpdateQuestion(interview, vector, questionId, (question) =>
            {
                question.IsFlagged = isFlagged;
            });
        }

        private InterviewData SetInterviewValidity(InterviewData interview, bool isValid)
        {
            interview.HasErrors = !isValid;
            return interview;
        }

        private InterviewData SaveComment(InterviewData interview, decimal[] vector, Guid questionId, string comment, Guid userId, string userName,
            DateTime commentTime)
        {
            var interviewQuestionComment = new InterviewQuestionComment()
            {
                Id = Guid.NewGuid(),
                Text = comment,
                CommenterId = userId,
                CommenterName = userName,
                Date = commentTime
            };

            return UpdateQuestion(interview, vector, questionId, (question) =>
            {
                if (question.Comments == null)
                    question.Comments = new List<InterviewQuestionComment>();
                question.Comments.Add(interviewQuestionComment);
            });
        }

        public InterviewEventHandlerFunctional(IReadSideRepositoryWriter<UserDocument> users, IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnriePropagationStructures,
            IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewData, ISynchronizationDataStorage syncStorage)
            : base(interviewData)
        {
            this.users = users;
            this.questionnriePropagationStructures = questionnriePropagationStructures;
            this.syncStorage = syncStorage;
        }

        public ViewWithSequence<InterviewData> Create(IPublishedEvent<InterviewCreated> evnt)
        {
            return CreateViewWithSequence(evnt.Payload.UserId, evnt.EventSourceId,
                evnt.EventTimeStamp, evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion,
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Create(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
           return CreateViewWithSequence(evnt.Payload.UserId, evnt.EventSourceId,
                evnt.EventTimeStamp, evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion, 
                evnt.EventSequence);
        }

        private ViewWithSequence<InterviewData> CreateViewWithSequence(Guid userId, Guid eventSourceId, DateTime eventTimeStamp,
            Guid questionnaireId, long questionnaireVersion, long eventSequence)
        {
            var responsible = this.users.GetById(userId);

            var interview = new InterviewData()
            {
                InterviewId = eventSourceId,
                UpdateDate = eventTimeStamp,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                ResponsibleId = userId, // Creator is responsible
                ResponsibleRole = responsible.Roles.FirstOrDefault()
            };
            var emptyVector = new decimal[0];
            interview.Levels.Add(CreateLevelIdFromPropagationVector(emptyVector), new InterviewLevel(eventSourceId, null, emptyVector));
            return new ViewWithSequence<InterviewData>(interview, eventSequence);
        }


        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<InterviewStatusChanged> evnt)
        {
            currentState.Document.Status = evnt.Payload.Status;

            if (!currentState.Document.WasCompleted && evnt.Payload.Status == InterviewStatus.Completed)
            {
                currentState.Document.WasCompleted = true;
            }

            currentState.Sequence = evnt.EventSequence;

            var newStatus = evnt.Payload.Status;

            if (IsInterviewWithStatusNeedToBeResendToCapi(newStatus))
            {
                ResendInterviewInNewStatus(currentState.Document, newStatus);
            }
            else if (IsInterviewWithStatusNeedToBeDeletedOnCapi(newStatus))
            {
                syncStorage.MarkInterviewForClientDeleting(evnt.EventSourceId, null);
            }
        
            return currentState;
        }

        private bool IsInterviewWithStatusNeedToBeResendToCapi(InterviewStatus newStatus)
        {
            return newStatus == InterviewStatus.RejectedBySupervisor;
        }

        private bool IsInterviewWithStatusNeedToBeDeletedOnCapi(InterviewStatus newStatus)
        {
            return newStatus == InterviewStatus.Completed || newStatus == InterviewStatus.Deleted;
        }

        public void ResendInterviewInNewStatus(InterviewData interview, InterviewStatus newStatus)
        {
            var interviewSyncData = BuildSynchronizationDtoWhichIsAssignedToUser(interview, interview.ResponsibleId, newStatus);

            syncStorage.SaveInterview(interviewSyncData, interview.ResponsibleId);
        }

        public void ResendInterviewForPerson(InterviewData interview, Guid responsibleId)
        {
            var interviewSyncData = BuildSynchronizationDtoWhichIsAssignedToUser(interview, responsibleId, InterviewStatus.InterviewerAssigned);

            syncStorage.SaveInterview(interviewSyncData, interview.ResponsibleId);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<SupervisorAssigned> evnt)
        {
            currentState.Document.ResponsibleId = evnt.Payload.SupervisorId;
            currentState.Document.ResponsibleRole = UserRoles.Supervisor;
            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<InterviewerAssigned> evnt)
        {
            currentState.Document.ResponsibleId = evnt.Payload.InterviewerId;
            currentState.Document.ResponsibleRole = UserRoles.Operator;
            currentState.Sequence = evnt.EventSequence;
            this.ResendInterviewForPerson(currentState.Document, evnt.Payload.InterviewerId);
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<RosterRowAdded> evnt)
        {
            var scopeOfCurrentGroup = GetScopeOfPassedGroup(currentState.Document, evnt.Payload.GroupId);

            this.AddLevelToInterview(currentState.Document, evnt.Payload.OuterRosterVector, evnt.Payload.RosterInstanceId, evnt.Payload.SortIndex, scopeOfCurrentGroup);

            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<RosterRowRemoved> evnt)
        {
            var scopeOfCurrentGroup = GetScopeOfPassedGroup(currentState.Document, evnt.Payload.GroupId);

            var newVector = CreateNewVector(evnt.Payload.OuterRosterVector, evnt.Payload.RosterInstanceId);
            var levelKey = CreateLevelIdFromPropagationVector(newVector);
            this.RemoveLevelFromInterview(currentState.Document, levelKey, new[] { evnt.Payload.GroupId }, scopeOfCurrentGroup.ScopeId);

            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<RosterInstancesAdded> evnt)
        {
            foreach (var instance in evnt.Payload.Instances)
            {
                var scopeOfCurrentGroup = GetScopeOfPassedGroup(currentState.Document, instance.GroupId);

                this.AddLevelToInterview(currentState.Document,
                    instance.OuterRosterVector, instance.RosterInstanceId, instance.SortIndex, scopeOfCurrentGroup);
            }

            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<RosterInstancesRemoved> evnt)
        {
            foreach (var instance in evnt.Payload.Instances)
            {
                var scopeOfCurrentGroup = GetScopeOfPassedGroup(currentState.Document, instance.GroupId);

                var rosterVector = CreateNewVector(instance.OuterRosterVector, instance.RosterInstanceId);
                var levelKey = CreateLevelIdFromPropagationVector(rosterVector);

                this.RemoveLevelFromInterview(currentState.Document, levelKey, new[] { instance.GroupId }, scopeOfCurrentGroup.ScopeId);
            }

            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<GroupPropagated> evnt)
        {
            var scopeOfCurrentGroup = GetScopeOfPassedGroup(currentState.Document,
                                                          evnt.Payload.GroupId);
            List<string> keysOfLevelsByScope =
                GetLevelsByScopeFromInterview(interview: currentState.Document, scopeId: scopeOfCurrentGroup.ScopeId);

            int countOfLevelByScope = keysOfLevelsByScope.Count();

            if (evnt.Payload.Count == countOfLevelByScope)
            {
                currentState.Sequence = evnt.EventSequence;
                return currentState;
            }

            if (countOfLevelByScope < evnt.Payload.Count)
            {
                AddNewLevelsToInterview(currentState.Document, startIndex: countOfLevelByScope,
                    count: evnt.Payload.Count - countOfLevelByScope,
                    outerVector: evnt.Payload.OuterScopePropagationVector, sortIndex: null, scope: scopeOfCurrentGroup);
            }
            else
            {
                Dictionary<string,Guid[]> keysOfLevelToBeDeleted =
                    keysOfLevelsByScope.Skip(evnt.Payload.Count)
                        .Take(countOfLevelByScope - evnt.Payload.Count)
                        .ToDictionary(keyOfLevelsByScope => keyOfLevelsByScope, keyOfLevelsByScope => scopeOfCurrentGroup.RosterIdToRosterTitleQuestionIdMap.Keys.ToArray());

                RemoveLevelsFromInterview(currentState.Document, keysOfLevelToBeDeleted, scopeOfCurrentGroup.ScopeId);
            }
            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<AnswerCommented> evnt)
        {
            var commenter = this.users.GetById(evnt.Payload.UserId);
            if (commenter == null)
                return currentState;

            return new ViewWithSequence<InterviewData>(SaveComment(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.Comment, evnt.Payload.UserId, commenter.UserName, evnt.Payload.CommentTime), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.SelectedValues), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.Answer), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.Answer), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.Answer), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<TextQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.Answer), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                  new InterviewTextListAnswers(evnt.Payload.Answers)), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.SelectedValue), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.SelectedPropagationVector), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.SelectedPropagationVectors), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.Answer), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy, evnt.Payload.Timestamp)), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.Answer), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<AnswerRemoved> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                UpdateQuestion(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, question =>
                {
                    question.Answer = null;
                    question.IsAnswered = false;
                }),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<AnswersRemoved> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                evnt.Payload.Questions.Aggregate(
                    currentState.Document,
                    (document, question) => UpdateQuestion(document, question.RosterVector, question.Id, updatedQuestion =>
                    {
                        updatedQuestion.Answer = null;
                        updatedQuestion.IsAnswered = false;
                    })),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<GroupDisabled> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                PreformActionOnLevel(currentState.Document, evnt.Payload.PropagationVector, level =>
                {
                    if (!level.DisabledGroups.Contains(evnt.Payload.GroupId))
                    {
                        level.DisabledGroups.Add(evnt.Payload.GroupId);
                    }
                }),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<GroupEnabled> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                PreformActionOnLevel(currentState.Document, evnt.Payload.PropagationVector, level =>
                {
                    if (level.DisabledGroups.Contains(evnt.Payload.GroupId))
                    {
                        level.DisabledGroups.Remove(evnt.Payload.GroupId);
                    }
                }),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<GroupsDisabled> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                evnt.Payload.Groups.Aggregate(
                    currentState.Document,
                    (document, group) => PreformActionOnLevel(document, group.RosterVector, level =>
                    {
                        if (!level.DisabledGroups.Contains(group.Id))
                        {
                            level.DisabledGroups.Add(group.Id);
                        }
                    })),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<GroupsEnabled> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                evnt.Payload.Groups.Aggregate(
                    currentState.Document,
                    (document, group) => PreformActionOnLevel(document, group.RosterVector, level =>
                    {
                        if (level.DisabledGroups.Contains(group.Id))
                        {
                            level.DisabledGroups.Remove(group.Id);
                        }
                    })),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<QuestionDisabled> evnt)
        {
            return
                new ViewWithSequence<InterviewData>(
                    ChangeQuestionConditionState(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                        false), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<QuestionEnabled> evnt)
        {
            return
                new ViewWithSequence<InterviewData>(
                    ChangeQuestionConditionState(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
               true), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<QuestionsDisabled> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                evnt.Payload.Questions.Aggregate(
                    currentState.Document,
                    (document, question) => ChangeQuestionConditionState(document, question.RosterVector, question.Id, false)),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<QuestionsEnabled> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                evnt.Payload.Questions.Aggregate(
                    currentState.Document,
                    (document, question) => ChangeQuestionConditionState(document, question.RosterVector, question.Id, true)),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<AnswerDeclaredInvalid> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                ChangeQuestionConditionValidity(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, false),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<AnswerDeclaredValid> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                ChangeQuestionConditionValidity(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, true),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<AnswersDeclaredInvalid> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                evnt.Payload.Questions.Aggregate(
                    currentState.Document,
                    (document, question) => ChangeQuestionConditionValidity(document, question.RosterVector, question.Id, false)),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<AnswersDeclaredValid> evnt)
        {
            return new ViewWithSequence<InterviewData>(
                evnt.Payload.Questions.Aggregate(
                    currentState.Document,
                    (document, question) => ChangeQuestionConditionValidity(document, question.RosterVector, question.Id, true)),
                evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<FlagRemovedFromAnswer> evnt)
        {
            return
                new ViewWithSequence<InterviewData>(
                    ChangeQuestionConditionValidity(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, false),
                    evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<FlagSetToAnswer> evnt)
        {
            return
                new ViewWithSequence<InterviewData>(
                    SetFlagStateForQuestion(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, true),
                    evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<InterviewDeclaredInvalid> evt)
        {
            return new ViewWithSequence<InterviewData>(this.SetInterviewValidity(currentState.Document, false), evt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<InterviewDeclaredValid> evt)
        {
            return new ViewWithSequence<InterviewData>(this.SetInterviewValidity(currentState.Document, true), evt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<RosterRowTitleChanged> evnt)
        {
            var newVector = CreateNewVector(evnt.Payload.OuterRosterVector, evnt.Payload.RosterInstanceId);

            return
                new ViewWithSequence<InterviewData>(PreformActionOnLevel(currentState.Document, newVector, (level) =>
                {
                    if (level.RosterRowTitles.ContainsKey(evnt.Payload.GroupId))
                    {
                        level.RosterRowTitles[evnt.Payload.GroupId] = evnt.Payload.Title;
                    }
                    else
                    {
                        level.RosterRowTitles.Add(evnt.Payload.GroupId, evnt.Payload.Title);
                    }

                }), evnt.EventSequence);
        }

        private InterviewSynchronizationDto BuildSynchronizationDtoWhichIsAssignedToUser(InterviewData interview, Guid userId,
            InterviewStatus status)
        {
            var answeredQuestions = new List<AnsweredQuestionSynchronizationDto>();
            var disabledGroups = new HashSet<InterviewItemId>();
            var disabledQuestions = new HashSet<InterviewItemId>();
            var validQuestions = new HashSet<InterviewItemId>();
            var invalidQuestions = new HashSet<InterviewItemId>();
            var propagatedGroupInstanceCounts = new Dictionary<InterviewItemId, RosterSynchronizationDto[]>();

            var questionnariePropagationStructure = this.questionnriePropagationStructures.GetById(interview.QuestionnaireId,
                interview.QuestionnaireVersion);

            foreach (var interviewLevel in interview.Levels.Values)
            {
                foreach (var interviewQuestion in interviewLevel.GetAllQuestions())
                {
                    var answeredQuestion = new AnsweredQuestionSynchronizationDto(interviewQuestion.Id, interviewLevel.RosterVector,
                        interviewQuestion.Answer,
                        interviewQuestion.Comments.Any()
                            ? interviewQuestion.Comments.Last().Text
                            : null);
                    answeredQuestions.Add(answeredQuestion);
                    if (!interviewQuestion.Enabled)
                        disabledQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.RosterVector));

#warning TLK: validness flag misses undefined state
                    if (!interviewQuestion.Valid)
                        invalidQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.RosterVector));
                    if (interviewQuestion.Valid)
                        validQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.RosterVector));
                }
                foreach (var disabledGroup in interviewLevel.DisabledGroups)
                {
                    disabledGroups.Add(new InterviewItemId(disabledGroup, interviewLevel.RosterVector));
                }

                FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(questionnariePropagationStructure, interviewLevel,
                    propagatedGroupInstanceCounts);
            }
            return new InterviewSynchronizationDto(interview.InterviewId,
                status,
                userId, interview.QuestionnaireId, interview.QuestionnaireVersion,
                answeredQuestions.ToArray(), disabledGroups, disabledQuestions,
                validQuestions, invalidQuestions, null, propagatedGroupInstanceCounts, interview.WasCompleted);
        }

        private void FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(
           QuestionnaireRosterStructure questionnarieRosterStructure, InterviewLevel interviewLevel,
           Dictionary<InterviewItemId, RosterSynchronizationDto[]> propagatedGroupInstanceCounts)
        {
            if (interviewLevel.RosterVector.Length == 0)
                return;

            var outerVector = CreateOuterVector(interviewLevel);

            foreach (var scopeId in interviewLevel.ScopeIds)
            {
                foreach (var groupId in questionnarieRosterStructure.RosterScopes[scopeId.Key].RosterIdToRosterTitleQuestionIdMap.Keys)
                {
                    var groupKey = new InterviewItemId(groupId, outerVector);

                    var rosterTitle = interviewLevel.RosterRowTitles.ContainsKey(groupId)
                        ? interviewLevel.RosterRowTitles[groupId]
                        : string.Empty;
                    AddPropagatedGroupToDictionary(propagatedGroupInstanceCounts, scopeId.Value, rosterTitle, interviewLevel.RosterVector.Last(), groupKey);
                }
            }
        }

        private void AddPropagatedGroupToDictionary(Dictionary<InterviewItemId, RosterSynchronizationDto[]> propagatedGroupInstanceCounts,
            int? sortIndex, string rosterTitle, decimal rosterInstanceId,
            InterviewItemId groupKey)
        {
            List<RosterSynchronizationDto> currentRosterInstances = propagatedGroupInstanceCounts.ContainsKey(groupKey) ? propagatedGroupInstanceCounts[groupKey].ToList() : new List<RosterSynchronizationDto>();

            currentRosterInstances.Add(new RosterSynchronizationDto(groupKey.Id,
                groupKey.InterviewItemPropagationVector, rosterInstanceId, sortIndex, rosterTitle));

            propagatedGroupInstanceCounts[groupKey] = currentRosterInstances.ToArray();
        }

        private decimal[] CreateOuterVector(InterviewLevel interviewLevel)
        {
            var outerVector = new decimal[interviewLevel.RosterVector.Length - 1];
            for (int i = 0; i < interviewLevel.RosterVector.Length - 1; i++)
            {
                outerVector[i] = interviewLevel.RosterVector[i];
            }
            return outerVector;
        }
        
    }
}
