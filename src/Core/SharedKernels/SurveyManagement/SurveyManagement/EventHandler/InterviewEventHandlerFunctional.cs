using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class InterviewEventHandlerFunctional : 
        AbstractFunctionalEventHandler<ViewWithSequence<InterviewData>>,
        ICreateHandler<ViewWithSequence<InterviewData>, InterviewCreated>,
        ICreateHandler<ViewWithSequence<InterviewData>, InterviewFromPreloadedDataCreated>,
        ICreateHandler<ViewWithSequence<InterviewData>, InterviewOnClientCreated>,
        IUpdateHandler<ViewWithSequence<InterviewData>, InterviewStatusChanged>,
        IUpdateHandler<ViewWithSequence<InterviewData>, SupervisorAssigned>,
        IUpdateHandler<ViewWithSequence<InterviewData>, InterviewerAssigned>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GroupPropagated>,
        IUpdateHandler<ViewWithSequence<InterviewData>, RosterInstancesTitleChanged>,
        IUpdateHandler<ViewWithSequence<InterviewData>, RosterInstancesAdded>,
        IUpdateHandler<ViewWithSequence<InterviewData>, RosterInstancesRemoved>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswerCommented>,
        IUpdateHandler<ViewWithSequence<InterviewData>, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, NumericRealQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, NumericIntegerQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, TextQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, TextListQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, SingleOptionQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, SingleOptionLinkedQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, DateTimeQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GeoLocationQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, QRBarcodeQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, PictureQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswersRemoved>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GroupsDisabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GroupsEnabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, QuestionsDisabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, QuestionsEnabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswersDeclaredInvalid>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswersDeclaredValid>,
        IUpdateHandler<ViewWithSequence<InterviewData>, FlagRemovedFromAnswer>,
        IUpdateHandler<ViewWithSequence<InterviewData>, FlagSetToAnswer>,
        IUpdateHandler<ViewWithSequence<InterviewData>, InterviewDeclaredInvalid>,
        IUpdateHandler<ViewWithSequence<InterviewData>, InterviewDeclaredValid>,
        IDeleteHandler<ViewWithSequence<InterviewData>, InterviewHardDeleted>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnriePropagationStructures;
      

        public override object[] Readers
        {
            get { return new object[] { users, questionnriePropagationStructures }; }
        }

        private static string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            if (vector.Length == 0)
                return "#";
            return EventHandlerUtils.CreateLeveKeyFromPropagationVector(vector);
        }

        private RosterScopeDescription GetScopeOfPassedGroup(InterviewData interview, Guid groupId)
        {
            var questionnarie = this.questionnriePropagationStructures.GetById(interview.QuestionnaireId, interview.QuestionnaireVersion);

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

        private void RemoveLevelsFromInterview(InterviewData interview, Dictionary<string, Guid[]> levelKeysForDelete, ValueVector<Guid> scopeVector)
        {
            foreach (var levelKey in levelKeysForDelete)
            {
                this.RemoveLevelFromInterview(interview, levelKey.Key, levelKey.Value, scopeVector);
            }
        }

        private void AddNewLevelsToInterview(InterviewData interview, int startIndex, int count, decimal[] outerVector, int? sortIndex, RosterScopeDescription scope)
        {
            for (int rosterInstanceId = startIndex; rosterInstanceId < startIndex + count; rosterInstanceId++)
            {
                this.AddLevelToInterview(interview, outerVector, rosterInstanceId, sortIndex, scope);
            }
        }

        private void RemoveLevelFromInterview(InterviewData interview, string levelKey, Guid[] groupIds, ValueVector<Guid> scopeVector)
        {
            if (interview.Levels.ContainsKey(levelKey))
            {
                var level = interview.Levels[levelKey];

                foreach (var groupId in groupIds)
                {
                    level.DisabledGroups.Remove(groupId);
                }

                if (!level.ScopeVectors.ContainsKey(scopeVector))
                    return;

                if (level.ScopeVectors.Count == 1)
                    interview.Levels.Remove(levelKey);
                else
                {
                    level.ScopeVectors.Remove(scopeVector);
                }
            }
        }

        private void AddLevelToInterview(InterviewData interview, decimal[] vector, decimal rosterInstanceId, int? sortIndex, RosterScopeDescription scope)
        {
            var newVector = this.CreateNewVector(vector, rosterInstanceId);
            var levelKey = CreateLevelIdFromPropagationVector(newVector);
            if (!interview.Levels.ContainsKey(levelKey))
                interview.Levels[levelKey] = new InterviewLevel(scope.ScopeVector, sortIndex, newVector);
            else
                interview.Levels[levelKey].ScopeVectors[scope.ScopeVector] = sortIndex;

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

        private List<string> GetLevelsByScopeFromInterview(InterviewData interview, ValueVector<Guid> scopeVector)
        {
            return interview.Levels.Where(level => level.Value.ScopeVectors.ContainsKey(scopeVector))
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

        public InterviewEventHandlerFunctional(IReadSideRepositoryWriter<UserDocument> users,
            IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnriePropagationStructures,
            IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewData)
            : base(interviewData)
        {
            this.users = users;
            this.questionnriePropagationStructures = questionnriePropagationStructures;
        }

        public ViewWithSequence<InterviewData> Create(IPublishedEvent<InterviewCreated> evnt)
        {
            return this.CreateViewWithSequence(evnt.Payload.UserId, evnt.EventSourceId,
                evnt.EventTimeStamp, evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion,
                evnt.EventSequence, false);
        }

        public ViewWithSequence<InterviewData> Create(IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            return this.CreateViewWithSequence(evnt.Payload.UserId, evnt.EventSourceId,
                evnt.EventTimeStamp, evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion,
                evnt.EventSequence, false);
        }

        public ViewWithSequence<InterviewData> Create(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
           return this.CreateViewWithSequence(evnt.Payload.UserId, evnt.EventSourceId,
                evnt.EventTimeStamp, evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion, 
                evnt.EventSequence, true);
        }

        private ViewWithSequence<InterviewData> CreateViewWithSequence(Guid userId, Guid eventSourceId, DateTime eventTimeStamp,
            Guid questionnaireId, long questionnaireVersion, long eventSequence, bool createdOnClient)
        {
            var responsible = this.users.GetById(userId);

            var interview = new InterviewData()
            {
                InterviewId = eventSourceId,
                UpdateDate = eventTimeStamp,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                ResponsibleId = userId, // Creator is responsible
                ResponsibleRole = responsible != null ? responsible.Roles.FirstOrDefault() : UserRoles.Undefined,
                CreatedOnClient = createdOnClient
            };
            var emptyVector = new decimal[0];
            interview.Levels.Add(CreateLevelIdFromPropagationVector(emptyVector), new InterviewLevel(new ValueVector<Guid>(), null, emptyVector));
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
        
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<SupervisorAssigned> evnt)
        {
            currentState.Document.ResponsibleId = evnt.Payload.SupervisorId;
            currentState.Document.SupervisorId = evnt.Payload.SupervisorId;
            currentState.Document.ResponsibleRole = UserRoles.Supervisor;
            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<InterviewerAssigned> evnt)
        {
            currentState.Document.ResponsibleId = evnt.Payload.InterviewerId;
            currentState.Document.ResponsibleRole = UserRoles.Operator;
            currentState.Sequence = evnt.EventSequence;

            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<RosterInstancesAdded> evnt)
        {
            foreach (var instance in evnt.Payload.Instances)
            {
                var scopeOfCurrentGroup = this.GetScopeOfPassedGroup(currentState.Document, instance.GroupId);

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
                var scopeOfCurrentGroup = this.GetScopeOfPassedGroup(currentState.Document, instance.GroupId);

                var rosterVector = this.CreateNewVector(instance.OuterRosterVector, instance.RosterInstanceId);
                var levelKey = CreateLevelIdFromPropagationVector(rosterVector);

                this.RemoveLevelFromInterview(currentState.Document, levelKey, new[] { instance.GroupId }, scopeOfCurrentGroup.ScopeVector);
            }

            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<GroupPropagated> evnt)
        {
            var scopeOfCurrentGroup = this.GetScopeOfPassedGroup(currentState.Document,
                                                          evnt.Payload.GroupId);
            List<string> keysOfLevelsByScope =
                this.GetLevelsByScopeFromInterview(interview: currentState.Document, scopeVector: scopeOfCurrentGroup.ScopeVector);

            int countOfLevelByScope = keysOfLevelsByScope.Count();

            if (evnt.Payload.Count == countOfLevelByScope)
            {
                currentState.Sequence = evnt.EventSequence;
                return currentState;
            }

            if (countOfLevelByScope < evnt.Payload.Count)
            {
                this.AddNewLevelsToInterview(currentState.Document, startIndex: countOfLevelByScope,
                    count: evnt.Payload.Count - countOfLevelByScope,
                    outerVector: evnt.Payload.OuterScopePropagationVector, sortIndex: null, scope: scopeOfCurrentGroup);
            }
            else
            {
                Dictionary<string,Guid[]> keysOfLevelToBeDeleted =
                    keysOfLevelsByScope.Skip(evnt.Payload.Count)
                        .Take(countOfLevelByScope - evnt.Payload.Count)
                        .ToDictionary(keyOfLevelsByScope => keyOfLevelsByScope, keyOfLevelsByScope => scopeOfCurrentGroup.RosterIdToRosterTitleQuestionIdMap.Keys.ToArray());

                this.RemoveLevelsFromInterview(currentState.Document, keysOfLevelToBeDeleted, scopeOfCurrentGroup.ScopeVector);
            }
            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<AnswerCommented> evnt)
        {
            var commenter = this.users.GetById(evnt.Payload.UserId);

            return new ViewWithSequence<InterviewData>(this.SaveComment(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.Comment, evnt.Payload.UserId, commenter != null ? commenter.UserName : "<Unknown user>", evnt.Payload.CommentTime), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.SelectedValues), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.Answer), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.Answer), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<TextQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.Answer), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                  new InterviewTextListAnswers(evnt.Payload.Answers)), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.SelectedValue), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.SelectedPropagationVector), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.SelectedPropagationVectors), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.Answer), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy, evnt.Payload.Altitude, 
                        evnt.Payload.Timestamp)), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.Answer), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<PictureQuestionAnswered> evnt)
        {
            return new ViewWithSequence<InterviewData>(this.SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.PictureFileName), evnt.EventSequence);
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
                   SetFlagStateForQuestion(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, false),
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

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<RosterInstancesTitleChanged> evnt)
        {
            foreach (var changedRosterRowTitleDto in evnt.Payload.ChangedInstances)
            {
                var newVector = this.CreateNewVector(changedRosterRowTitleDto.RosterInstance.OuterRosterVector, changedRosterRowTitleDto.RosterInstance.RosterInstanceId);
                ChangedRosterInstanceTitleDto dto = changedRosterRowTitleDto;

                PreformActionOnLevel(currentState.Document, newVector, (level) =>
                {
                    if (level.RosterRowTitles.ContainsKey(dto.RosterInstance.GroupId))
                    {
                        level.RosterRowTitles[dto.RosterInstance.GroupId] = dto.Title;
                    }
                    else
                    {
                        level.RosterRowTitles.Add(dto.RosterInstance.GroupId, dto.Title);
                    }
                });
            }

            return new ViewWithSequence<InterviewData>(currentState.Document, evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Delete(ViewWithSequence<InterviewData> currentState, IPublishedEvent<InterviewHardDeleted> evnt)
        {
            return currentState;
        }
    }
}
