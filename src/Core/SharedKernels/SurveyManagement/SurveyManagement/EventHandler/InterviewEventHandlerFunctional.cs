using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
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
    internal class InterviewEventHandlerFunctional :
        AbstractFunctionalEventHandler<InterviewData, IReadSideKeyValueStorage<InterviewData>>,
        IUpdateHandler<InterviewData, InterviewCreated>,
        IUpdateHandler<InterviewData, InterviewFromPreloadedDataCreated>,
        IUpdateHandler<InterviewData, InterviewOnClientCreated>,
        IUpdateHandler<InterviewData, InterviewStatusChanged>,
        IUpdateHandler<InterviewData, SupervisorAssigned>,
        IUpdateHandler<InterviewData, InterviewerAssigned>,
        IUpdateHandler<InterviewData, GroupPropagated>,
        IUpdateHandler<InterviewData, RosterInstancesTitleChanged>,
        IUpdateHandler<InterviewData, RosterInstancesAdded>,
        IUpdateHandler<InterviewData, RosterInstancesRemoved>,
        IUpdateHandler<InterviewData, AnswerCommented>,
        IUpdateHandler<InterviewData, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<InterviewData, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewData, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewData, TextQuestionAnswered>,
        IUpdateHandler<InterviewData, TextListQuestionAnswered>,
        IUpdateHandler<InterviewData, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewData, SingleOptionLinkedQuestionAnswered>,
        IUpdateHandler<InterviewData, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateHandler<InterviewData, DateTimeQuestionAnswered>,
        IUpdateHandler<InterviewData, GeoLocationQuestionAnswered>,
        IUpdateHandler<InterviewData, QRBarcodeQuestionAnswered>,
        IUpdateHandler<InterviewData, PictureQuestionAnswered>,
        IUpdateHandler<InterviewData, AnswersRemoved>,
        IUpdateHandler<InterviewData, GroupsDisabled>,
        IUpdateHandler<InterviewData, GroupsEnabled>,
        IUpdateHandler<InterviewData, QuestionsDisabled>,
        IUpdateHandler<InterviewData, QuestionsEnabled>,
        IUpdateHandler<InterviewData, AnswersDeclaredInvalid>,
        IUpdateHandler<InterviewData, AnswersDeclaredValid>,
        IUpdateHandler<InterviewData, FlagRemovedFromAnswer>,
        IUpdateHandler<InterviewData, FlagSetToAnswer>,
        IUpdateHandler<InterviewData, InterviewDeclaredInvalid>,
        IUpdateHandler<InterviewData, InterviewDeclaredValid>,
        IUpdateHandler<InterviewData, InterviewHardDeleted>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures;


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

        private RosterScopeDescription GetScopeOfPassedGroup(InterviewData interview, Guid groupId, QuestionnaireRosterStructure questionnarie)
        {
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
                if (!questionsAtTheLevel.QuestionsSearchCahche.ContainsKey(questionId))
                    questionsAtTheLevel.QuestionsSearchCahche.Add(questionId, new InterviewQuestion(questionId));

                var answeredQuestion = questionsAtTheLevel.QuestionsSearchCahche[questionId];

                update(answeredQuestion);
            });
        }

        private static InterviewData ChangeQuestionConditionState(InterviewData interview, decimal[] vector, Guid questionId, bool disabled)
        {
            return UpdateQuestion(interview, vector, questionId, (question) =>
            {
                if (disabled)
                    question.QuestionState &= ~QuestionState.Enabled;
                else
                    question.QuestionState = question.QuestionState | QuestionState.Enabled;
            });
        }

        private static InterviewData ChangeQuestionConditionValidity(InterviewData interview, decimal[] vector, Guid questionId, bool invalid)
        {
            return UpdateQuestion(interview, vector, questionId, (question) =>
            {
                if (invalid)
                    question.QuestionState &= ~QuestionState.Valid; 
                else
                    question.QuestionState = question.QuestionState | QuestionState.Valid;
            });
        }

        private InterviewData SaveAnswer<T>(InterviewData interview, decimal[] vector, Guid questionId, T answer)
        {
            return PreformActionOnLevel(interview, vector, (level) =>
            {
                InterviewQuestion answeredQuestion;
                if (level.QuestionsSearchCahche.ContainsKey(questionId))
                    answeredQuestion = level.QuestionsSearchCahche[questionId];
                else
                {
                    answeredQuestion = new InterviewQuestion(questionId);
                    level.QuestionsSearchCahche.Add(questionId, answeredQuestion);
                }
                
                answeredQuestion.Answer = answer;

                answeredQuestion.QuestionState = answeredQuestion.QuestionState | QuestionState.Answered;
            });
        }

        private static InterviewData SetFlagStateForQuestion(InterviewData interview, decimal[] vector, Guid questionId, bool isFlagged)
        {
            return UpdateQuestion(interview, vector, questionId, (question) =>
            {
                if (isFlagged)
                    question.QuestionState = question.QuestionState | QuestionState.Flagged;
                else
                    question.QuestionState &= ~QuestionState.Flagged;
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
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures,
            IReadSideKeyValueStorage<InterviewData> interviewData)
            : base(interviewData)
        {
            this.users = users;
            this.questionnriePropagationStructures = questionnriePropagationStructures;
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<InterviewCreated> evnt)
        {
            return this.CreateViewWithSequence(evnt.Payload.UserId, evnt.EventSourceId,
                evnt.EventTimeStamp, evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion,
                evnt.EventSequence, false);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            return this.CreateViewWithSequence(evnt.Payload.UserId, evnt.EventSourceId,
                evnt.EventTimeStamp, evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion,
                evnt.EventSequence, false);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            return this.CreateViewWithSequence(evnt.Payload.UserId, evnt.EventSourceId,
                 evnt.EventTimeStamp, evnt.Payload.QuestionnaireId,
                 evnt.Payload.QuestionnaireVersion,
                 evnt.EventSequence, true);
        }

        private InterviewData CreateViewWithSequence(Guid userId, Guid eventSourceId, DateTime eventTimeStamp,
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
            return interview;
        }


        public InterviewData Update(InterviewData currentState, IPublishedEvent<InterviewStatusChanged> evnt)
        {
            currentState.Status = evnt.Payload.Status;

            if (!currentState.WasCompleted && evnt.Payload.Status == InterviewStatus.Completed)
            {
                currentState.WasCompleted = true;
            }
            
            return currentState;
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<SupervisorAssigned> evnt)
        {
            currentState.ResponsibleId = evnt.Payload.SupervisorId;
            currentState.SupervisorId = evnt.Payload.SupervisorId;
            currentState.ResponsibleRole = UserRoles.Supervisor;
            return currentState;
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<InterviewerAssigned> evnt)
        {
            currentState.ResponsibleId = evnt.Payload.InterviewerId;
            currentState.ResponsibleRole = UserRoles.Operator;

            return currentState;
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<RosterInstancesAdded> evnt)
        {
            var questionnarie = this.questionnriePropagationStructures.AsVersioned().Get(currentState.QuestionnaireId.FormatGuid(), currentState.QuestionnaireVersion);

            foreach (var instance in evnt.Payload.Instances)
            {
                var scopeOfCurrentGroup = this.GetScopeOfPassedGroup(currentState, instance.GroupId, questionnarie);

                this.AddLevelToInterview(currentState,
                    instance.OuterRosterVector, instance.RosterInstanceId, instance.SortIndex, scopeOfCurrentGroup);
            }

            return currentState;
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<RosterInstancesRemoved> evnt)
        {

            var questionnarie = this.questionnriePropagationStructures.AsVersioned().Get(currentState.QuestionnaireId.FormatGuid(), currentState.QuestionnaireVersion);
            foreach (var instance in evnt.Payload.Instances)
            {
                var scopeOfCurrentGroup = this.GetScopeOfPassedGroup(currentState, instance.GroupId, questionnarie);

                var rosterVector = this.CreateNewVector(instance.OuterRosterVector, instance.RosterInstanceId);
                var levelKey = CreateLevelIdFromPropagationVector(rosterVector);

                this.RemoveLevelFromInterview(currentState, levelKey, new[] { instance.GroupId }, scopeOfCurrentGroup.ScopeVector);
            }
            return currentState;
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<GroupPropagated> evnt)
        {

            var questionnarie = this.questionnriePropagationStructures.AsVersioned().Get(currentState.QuestionnaireId.FormatGuid(), currentState.QuestionnaireVersion);
            var scopeOfCurrentGroup = this.GetScopeOfPassedGroup(currentState,
                                                          evnt.Payload.GroupId, questionnarie);
            List<string> keysOfLevelsByScope =
                this.GetLevelsByScopeFromInterview(interview: currentState, scopeVector: scopeOfCurrentGroup.ScopeVector);

            int countOfLevelByScope = keysOfLevelsByScope.Count();

            if (evnt.Payload.Count == countOfLevelByScope)
            {
                return currentState;
            }

            if (countOfLevelByScope < evnt.Payload.Count)
            {
                this.AddNewLevelsToInterview(currentState, startIndex: countOfLevelByScope,
                    count: evnt.Payload.Count - countOfLevelByScope,
                    outerVector: evnt.Payload.OuterScopePropagationVector, sortIndex: null, scope: scopeOfCurrentGroup);
            }
            else
            {
                Dictionary<string, Guid[]> keysOfLevelToBeDeleted =
                    keysOfLevelsByScope.Skip(evnt.Payload.Count)
                        .Take(countOfLevelByScope - evnt.Payload.Count)
                        .ToDictionary(keyOfLevelsByScope => keyOfLevelsByScope, keyOfLevelsByScope => scopeOfCurrentGroup.RosterIdToRosterTitleQuestionIdMap.Keys.ToArray());

                this.RemoveLevelsFromInterview(currentState, keysOfLevelToBeDeleted, scopeOfCurrentGroup.ScopeVector);
            }
            return currentState;
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<AnswerCommented> evnt)
        {
            var commenter = this.users.GetById(evnt.Payload.UserId);

            return this.SaveComment(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.Comment, evnt.Payload.UserId, commenter != null ? commenter.UserName : "<Unknown user>", evnt.Payload.CommentTime);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.SelectedValues);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.Answer);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.Answer);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<TextQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.Answer);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                  new InterviewTextListAnswers(evnt.Payload.Answers));
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.SelectedValue);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.SelectedPropagationVector);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
            evnt.Payload.SelectedPropagationVectors);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
         evnt.Payload.Answer);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy, evnt.Payload.Altitude,
                    evnt.Payload.Timestamp));
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
       evnt.Payload.Answer);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<PictureQuestionAnswered> evnt)
        {
            return this.SaveAnswer(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
       evnt.Payload.PictureFileName);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<AnswersRemoved> evnt)
        {
            return 
                evnt.Payload.Questions.Aggregate(
                    currentState,
                    (document, question) => UpdateQuestion(document, question.RosterVector, question.Id, updatedQuestion =>
                    {
                        updatedQuestion.Answer = null;

                        updatedQuestion.QuestionState &= ~QuestionState.Answered;
                    }));
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<GroupsDisabled> evnt)
        {
            return evnt.Payload.Groups.Aggregate(
                    currentState,
                    (document, group) => PreformActionOnLevel(document, group.RosterVector, level =>
                    {
                        if (!level.DisabledGroups.Contains(group.Id))
                        {
                            level.DisabledGroups.Add(group.Id);
                        }
                    }));
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<GroupsEnabled> evnt)
        {
            return 
                evnt.Payload.Groups.Aggregate(
                    currentState,
                    (document, group) => PreformActionOnLevel(document, group.RosterVector, level =>
                    {
                        if (level.DisabledGroups.Contains(group.Id))
                        {
                            level.DisabledGroups.Remove(group.Id);
                        }
                    }));
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<QuestionsDisabled> evnt)
        {
            return 
                evnt.Payload.Questions.Aggregate(
                    currentState,
                    (document, question) => ChangeQuestionConditionState(document, question.RosterVector, question.Id, true));
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<QuestionsEnabled> evnt)
        {
            return 
                evnt.Payload.Questions.Aggregate(
                    currentState,
                    (document, question) => ChangeQuestionConditionState(document, question.RosterVector, question.Id, false));
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<AnswersDeclaredInvalid> evnt)
        {
            return
                evnt.Payload.Questions.Aggregate(
                    currentState,
                    (document, question) => ChangeQuestionConditionValidity(document, question.RosterVector, question.Id, true));
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<AnswersDeclaredValid> evnt)
        {
            return 
                evnt.Payload.Questions.Aggregate(
                    currentState,
                    (document, question) => ChangeQuestionConditionValidity(document, question.RosterVector, question.Id, false));
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<FlagRemovedFromAnswer> evnt)
        {
            return
                   SetFlagStateForQuestion(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, false);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<FlagSetToAnswer> evnt)
        {
            return
                 SetFlagStateForQuestion(currentState, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, true);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<InterviewDeclaredInvalid> evt)
        {
            return this.SetInterviewValidity(currentState, false);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<InterviewDeclaredValid> evt)
        {
            return this.SetInterviewValidity(currentState, true);
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<RosterInstancesTitleChanged> evnt)
        {
            foreach (var changedRosterRowTitleDto in evnt.Payload.ChangedInstances)
            {
                var newVector = this.CreateNewVector(changedRosterRowTitleDto.RosterInstance.OuterRosterVector, changedRosterRowTitleDto.RosterInstance.RosterInstanceId);
                ChangedRosterInstanceTitleDto dto = changedRosterRowTitleDto;

                PreformActionOnLevel(currentState, newVector, (level) =>
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

            return currentState;
        }

        public InterviewData Update(InterviewData currentState, IPublishedEvent<InterviewHardDeleted> evnt)
        {
            return null;
        }
    }
}
