using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewEventHandlerFunctional :
        AbstractFunctionalEventHandler<InterviewData, IReadSideKeyValueStorage<InterviewData>>,
        IUpdateHandler<InterviewData, InterviewCreated>,
        IUpdateHandler<InterviewData, InterviewFromPreloadedDataCreated>,
        IUpdateHandler<InterviewData, InterviewOnClientCreated>,
        IUpdateHandler<InterviewData, InterviewStatusChanged>,
        IUpdateHandler<InterviewData, InterviewReceivedByInterviewer>,
        IUpdateHandler<InterviewData, InterviewReceivedBySupervisor>,
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
        IUpdateHandler<InterviewData, YesNoQuestionAnswered>,
        IUpdateHandler<InterviewData, AnswersRemoved>,
        IUpdateHandler<InterviewData, GroupsDisabled>,
        IUpdateHandler<InterviewData, GroupsEnabled>,
        IUpdateHandler<InterviewData, StaticTextsEnabled>,
        IUpdateHandler<InterviewData, StaticTextsDisabled>,
        IUpdateHandler<InterviewData, StaticTextsDeclaredInvalid>,
        IUpdateHandler<InterviewData, StaticTextsDeclaredValid>,
        IUpdateHandler<InterviewData, QuestionsDisabled>,
        IUpdateHandler<InterviewData, QuestionsEnabled>,
        IUpdateHandler<InterviewData, AnswersDeclaredInvalid>,
        IUpdateHandler<InterviewData, AnswersDeclaredValid>,
        IUpdateHandler<InterviewData, FlagRemovedFromAnswer>,
        IUpdateHandler<InterviewData, FlagSetToAnswer>,
        IUpdateHandler<InterviewData, InterviewDeclaredInvalid>,
        IUpdateHandler<InterviewData, InterviewDeclaredValid>,
        IUpdateHandler<InterviewData, InterviewHardDeleted>,
        IUpdateHandler<InterviewData, AnswerRemoved>,

        IUpdateHandler<InterviewData, VariablesChanged>,
        IUpdateHandler<InterviewData, VariablesDisabled>,
        IUpdateHandler<InterviewData, VariablesEnabled>,
        IUpdateHandler<InterviewData, TranslationSwitched>
    {
        private readonly IPlainStorageAccessor<UserDocument> users;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IRostrerStructureService rostrerStructureService;

        public override object[] Readers
        {
            get { return new object[] { this.users }; }
        }

        private static string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            if (vector.Length == 0)
                return "#";
            return vector.CreateLeveKeyFromPropagationVector();
        }

        private RosterScopeDescription GetScopeOfPassedGroup(InterviewData interview, Guid groupId, Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes)
        {
            foreach (var scopeId in rosterScopes.Keys)
            {
                if (rosterScopes[scopeId].RosterIdToRosterTitleQuestionIdMap.ContainsKey(groupId))
                {
                    return rosterScopes[scopeId];
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
                if (!questionsAtTheLevel.QuestionsSearchCache.ContainsKey(questionId))
                    questionsAtTheLevel.QuestionsSearchCache.Add(questionId, new InterviewQuestion(questionId));

                var answeredQuestion = questionsAtTheLevel.QuestionsSearchCache[questionId];

                update(answeredQuestion);
            });
        }

        private static InterviewData UpdateStaticText(InterviewData interview, decimal[] vector, Guid staticTextId, Action<InterviewStaticText> update)
        {
            return PreformActionOnLevel(interview, vector, (interviewLevel) =>
            {
                if (!interviewLevel.StaticTexts.ContainsKey(staticTextId))
                    interviewLevel.StaticTexts.Add(staticTextId, new InterviewStaticText(staticTextId));

                var staticText = interviewLevel.StaticTexts[staticTextId];

                update(staticText);
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

        private static InterviewData ChangeQuestionConditionValidity(InterviewData interview, 
            RosterVector vector, 
            Guid questionId, 
            bool invalid, IReadOnlyList<FailedValidationCondition> failedValidationCondition)
        {
            return UpdateQuestion(interview, vector, questionId, (question) =>
            {
                if (invalid)
                    question.QuestionState &= ~QuestionState.Valid; 
                else
                    question.QuestionState = question.QuestionState | QuestionState.Valid;
                question.FailedValidationConditions = failedValidationCondition;
            });
        }

        private InterviewData SaveAnswer<T>(InterviewData interview, decimal[] vector, Guid questionId, T answer, bool treatAsAnswered)
        {
            return PreformActionOnLevel(interview, vector, (level) =>
            {
                InterviewQuestion answeredQuestion;
                if (level.QuestionsSearchCache.ContainsKey(questionId))
                    answeredQuestion = level.QuestionsSearchCache[questionId];
                else
                {
                    answeredQuestion = new InterviewQuestion(questionId);
                    level.QuestionsSearchCache.Add(questionId, answeredQuestion);
                }

                answeredQuestion.Answer = answer;

                if (!treatAsAnswered)
                {
                    answeredQuestion.QuestionState = answeredQuestion.QuestionState.Without(QuestionState.Answered);
                }
                else
                {
                    answeredQuestion.QuestionState = answeredQuestion.QuestionState.With(QuestionState.Answered);
                }
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

        private InterviewData SaveComment(InterviewData interview, decimal[] vector, Guid questionId, string comment, Guid userId, string userName, DateTime commentTime, UserRoles commenterRole)
        {
            var interviewQuestionComment = new InterviewQuestionComment()
            {
                Id = Guid.NewGuid(),
                Text = comment,
                CommenterId = userId,
                CommenterName = userName,
                Date = commentTime,
                CommenterRole = commenterRole
            };

            return UpdateQuestion(interview, vector, questionId, (question) =>
            {
                if (question.Comments == null)
                    question.Comments = new List<InterviewQuestionComment>();
                question.Comments.Add(interviewQuestionComment);
            });
        }

        public InterviewEventHandlerFunctional(
            IPlainStorageAccessor<UserDocument> users,
            IReadSideKeyValueStorage<InterviewData> interviewData,
            IQuestionnaireStorage questionnaireStorage,
            IRostrerStructureService rostrerStructureService)
            : base(interviewData)
        {
            this.users = users;
            this.questionnaireStorage = questionnaireStorage;
            this.rostrerStructureService = rostrerStructureService;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<InterviewCreated> @event)
        {
            return this.CreateViewWithSequence(@event.Payload.UserId, @event.EventSourceId,
                @event.EventTimeStamp, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion,
                @event.EventSequence, false);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<InterviewFromPreloadedDataCreated> @event)
        {
            return this.CreateViewWithSequence(@event.Payload.UserId, @event.EventSourceId,
                @event.EventTimeStamp, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion,
                @event.EventSequence, false);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<InterviewOnClientCreated> @event)
        {
            return this.CreateViewWithSequence(@event.Payload.UserId, @event.EventSourceId,
                 @event.EventTimeStamp, @event.Payload.QuestionnaireId,
                 @event.Payload.QuestionnaireVersion,
                 @event.EventSequence, true);
        }

        private InterviewData CreateViewWithSequence(Guid userId, Guid eventSourceId, DateTime eventTimeStamp,
            Guid questionnaireId, long questionnaireVersion, long eventSequence, bool createdOnClient)
        {
            var responsible = this.users.GetById(userId.FormatGuid());

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


        public InterviewData Update(InterviewData state, IPublishedEvent<InterviewStatusChanged> @event)
        {
            state.Status = @event.Payload.Status;

            if (!state.WasCompleted && @event.Payload.Status == InterviewStatus.Completed)
            {
                state.WasCompleted = true;
            }
            
            return state;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<SupervisorAssigned> @event)
        {
            state.ResponsibleId = @event.Payload.SupervisorId;
            state.SupervisorId = @event.Payload.SupervisorId;
            state.ResponsibleRole = UserRoles.Supervisor;
            state.IsMissingAssignToInterviewer = true;
            return state;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<InterviewerAssigned> @event)
        {
            if (@event.Payload.InterviewerId.HasValue)
            {
                state.ResponsibleId = @event.Payload.InterviewerId.Value;
                state.ResponsibleRole = UserRoles.Operator;
                state.ReceivedByInterviewer = false;
                state.IsMissingAssignToInterviewer = false;
            }
            else
            {
                state.ResponsibleId = state.SupervisorId.Value;
                state.ResponsibleRole = UserRoles.Supervisor;
                state.ReceivedByInterviewer = false;
                state.IsMissingAssignToInterviewer = true;
            }

            return state;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<RosterInstancesAdded> @event)
        {
            var questionnarie =
                this.questionnaireStorage.GetQuestionnaireDocument(new QuestionnaireIdentity(state.QuestionnaireId,
                    state.QuestionnaireVersion));
            if (questionnarie != null)
            {
                var rosters =  this.rostrerStructureService.GetRosterScopes(questionnarie);

                foreach (var instance in @event.Payload.Instances)
                {
                    var scopeOfCurrentGroup = this.GetScopeOfPassedGroup(state, instance.GroupId,rosters);
                    this.AddLevelToInterview(state,
                        instance.OuterRosterVector, instance.RosterInstanceId, instance.SortIndex, scopeOfCurrentGroup);
                }
            }

            return state;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<RosterInstancesRemoved> @event)
        {
            var questionnarie = this.questionnaireStorage.GetQuestionnaireDocument(new QuestionnaireIdentity(state.QuestionnaireId, state.QuestionnaireVersion));
            if (questionnarie != null)
            {
                var rosters = this.rostrerStructureService.GetRosterScopes(questionnarie);

                foreach (var instance in @event.Payload.Instances)
                {
                    var scopeOfCurrentGroup = this.GetScopeOfPassedGroup(state, instance.GroupId, rosters);

                    var rosterVector = this.CreateNewVector(instance.OuterRosterVector, instance.RosterInstanceId);
                    var levelKey = CreateLevelIdFromPropagationVector(rosterVector);

                    this.RemoveLevelFromInterview(state, levelKey, new[] {instance.GroupId},
                        scopeOfCurrentGroup.ScopeVector);
                }
            }
            return state;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<GroupPropagated> @event)
        {
            var questionnarie =
                this.questionnaireStorage.GetQuestionnaireDocument(new QuestionnaireIdentity(state.QuestionnaireId,
                    state.QuestionnaireVersion));
            if (questionnarie != null)
            {
                var rosters = this.rostrerStructureService.GetRosterScopes(questionnarie);
                var scopeOfCurrentGroup = this.GetScopeOfPassedGroup(state, @event.Payload.GroupId, rosters);
                List<string> keysOfLevelsByScope =
                    this.GetLevelsByScopeFromInterview(interview: state, scopeVector: scopeOfCurrentGroup.ScopeVector);

                int countOfLevelByScope = keysOfLevelsByScope.Count();

                if (@event.Payload.Count == countOfLevelByScope)
                {
                    return state;
                }

                if (countOfLevelByScope < @event.Payload.Count)
                {
                    this.AddNewLevelsToInterview(state, startIndex: countOfLevelByScope,
                        count: @event.Payload.Count - countOfLevelByScope,
                        outerVector: @event.Payload.OuterScopeRosterVector, sortIndex: null, scope: scopeOfCurrentGroup);
                }
                else
                {
                    Dictionary<string, Guid[]> keysOfLevelToBeDeleted =
                        keysOfLevelsByScope.Skip(@event.Payload.Count)
                            .Take(countOfLevelByScope - @event.Payload.Count)
                            .ToDictionary(keyOfLevelsByScope => keyOfLevelsByScope,
                                keyOfLevelsByScope =>
                                        scopeOfCurrentGroup.RosterIdToRosterTitleQuestionIdMap.Keys.ToArray());

                    this.RemoveLevelsFromInterview(state, keysOfLevelToBeDeleted, scopeOfCurrentGroup.ScopeVector);
                }
            }
            
            return state;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<AnswerCommented> @event)
        {
            var commenter = this.users.GetById(@event.Payload.UserId.FormatGuid());
            
            return this.SaveComment(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                @event.Payload.Comment, @event.Payload.UserId, commenter != null ? commenter.UserName : "<Unknown user>", @event.Payload.CommentTime,
                commenter.Roles.FirstOrDefault());
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                @event.Payload.SelectedValues, @event.Payload.SelectedValues?.Length > 0);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                @event.Payload.Answer, true);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                    @event.Payload.Answer, true);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<TextQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                    @event.Payload.Answer, @event.Payload.Answer != null);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<TextListQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                  new InterviewTextListAnswers(@event.Payload.Answers), true);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                @event.Payload.SelectedValue, true);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                @event.Payload.SelectedRosterVector, @event.Payload.SelectedRosterVector?.Length > 0);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                @event.Payload.SelectedRosterVectors, @event.Payload.SelectedRosterVectors?.Length > 0);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                @event.Payload.Answer, true);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                new GeoPosition(@event.Payload.Latitude, @event.Payload.Longitude, @event.Payload.Accuracy, @event.Payload.Altitude,
                    @event.Payload.Timestamp), true);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                @event.Payload.Answer, @event.Payload.Answer != null);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<PictureQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId,
                @event.Payload.PictureFileName, true);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<YesNoQuestionAnswered> @event)
        {
            return this.SaveAnswer(state, @event.Payload.RosterVector, @event.Payload.QuestionId, @event.Payload.AnsweredOptions,
                @event.Payload.AnsweredOptions?.Length > 0);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<AnswersRemoved> @event)
        {
            return 
                @event.Payload.Questions.Aggregate(
                    state,
                    (document, question) => UpdateQuestion(document, question.RosterVector, question.Id, updatedQuestion =>
                    {
                        updatedQuestion.Answer = null;

                        updatedQuestion.QuestionState &= ~QuestionState.Answered;
                    }));
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<AnswerRemoved> @event)
        {
            return UpdateQuestion(state, @event.Payload.RosterVector, @event.Payload.QuestionId, updatedQuestion =>
            {
                updatedQuestion.Answer = null;

                updatedQuestion.QuestionState &= ~QuestionState.Answered;
            });
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<StaticTextsEnabled> @event)
        {
            return @event.Payload.StaticTexts.Aggregate(
                state,
                (document, staticTextIdentity) => UpdateStaticText(document, staticTextIdentity.RosterVector, staticTextIdentity.Id, (staticText) =>
                {
                    staticText.IsEnabled = true;
                }));
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<StaticTextsDisabled> @event)
        {
            return @event.Payload.StaticTexts.Aggregate(
                state,
                (document, staticTextIdentity) => UpdateStaticText(document, staticTextIdentity.RosterVector, staticTextIdentity.Id, (staticText) =>
                {
                    staticText.IsEnabled = false;
                }));
        }


        public InterviewData Update(InterviewData state, IPublishedEvent<StaticTextsDeclaredInvalid> @event)
        {
            return @event.Payload.FailedValidationConditions.Aggregate(
                state,
                (document, staticTextKeyValue) => UpdateStaticText(document, staticTextKeyValue.Key.RosterVector, staticTextKeyValue.Key.Id, (staticText) =>
                {
                    staticText.IsInvalid = true;
                    staticText.FailedValidationConditions = staticTextKeyValue.Value.ToList();
                }));
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<StaticTextsDeclaredValid> @event)
        {
            return @event.Payload.StaticTexts.Aggregate(
                state,
                (document, staticTextIdentity) => UpdateStaticText(document, staticTextIdentity.RosterVector, staticTextIdentity.Id, (staticText) =>
                {
                    staticText.IsInvalid = false;
                    staticText.FailedValidationConditions = new List<FailedValidationCondition>();
                }));
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<GroupsDisabled> @event)
        {
            return @event.Payload.Groups.Aggregate(
                    state,
                    (document, group) => PreformActionOnLevel(document, group.RosterVector, level =>
                    {
                        if (!level.DisabledGroups.Contains(group.Id))
                        {
                            level.DisabledGroups.Add(group.Id);
                        }
                    }));
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<GroupsEnabled> @event)
        {
            return 
                @event.Payload.Groups.Aggregate(
                    state,
                    (document, group) => PreformActionOnLevel(document, group.RosterVector, level =>
                    {
                        if (level.DisabledGroups.Contains(group.Id))
                        {
                            level.DisabledGroups.Remove(group.Id);
                        }
                    }));
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<QuestionsDisabled> @event)
        {
            return 
                @event.Payload.Questions.Aggregate(
                    state,
                    (document, question) => ChangeQuestionConditionState(document, question.RosterVector, question.Id, true));
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<QuestionsEnabled> @event)
        {
            return 
                @event.Payload.Questions.Aggregate(
                    state,
                    (document, question) => ChangeQuestionConditionState(document, question.RosterVector, question.Id, false));
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<AnswersDeclaredInvalid> @event)
        {
            return
                @event.Payload.FailedValidationConditions.Keys.Aggregate(
                    state,
                    (document, question) => ChangeQuestionConditionValidity(document, question.RosterVector, question.Id, true, @event.Payload.FailedValidationConditions[question]));
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<AnswersDeclaredValid> @event)
        {
            return 
                @event.Payload.Questions.Aggregate(
                    state,
                    (document, question) => ChangeQuestionConditionValidity(document, question.RosterVector, question.Id, false, new FailedValidationCondition[] {}));
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<FlagRemovedFromAnswer> @event)
        {
            return
                   SetFlagStateForQuestion(state, @event.Payload.RosterVector, @event.Payload.QuestionId, false);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<FlagSetToAnswer> @event)
        {
            return
                 SetFlagStateForQuestion(state, @event.Payload.RosterVector, @event.Payload.QuestionId, true);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<InterviewDeclaredInvalid> @event)
        {
            return this.SetInterviewValidity(state, false);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<InterviewDeclaredValid> @event)
        {
            return this.SetInterviewValidity(state, true);
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<RosterInstancesTitleChanged> @event)
        {
            foreach (var changedRosterRowTitleDto in @event.Payload.ChangedInstances)
            {
                var newVector = this.CreateNewVector(changedRosterRowTitleDto.RosterInstance.OuterRosterVector, changedRosterRowTitleDto.RosterInstance.RosterInstanceId);
                ChangedRosterInstanceTitleDto dto = changedRosterRowTitleDto;

                PreformActionOnLevel(state, newVector, (level) =>
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

            return state;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<InterviewHardDeleted> @event)
        {
            return null;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<InterviewReceivedByInterviewer> @event)
        {
            state.ReceivedByInterviewer = true;
            return state;
        }

        public InterviewData Update(InterviewData state, IPublishedEvent<InterviewReceivedBySupervisor> @event)
        {
            state.ReceivedByInterviewer = false;
            return state;
        }

        public InterviewData Update(InterviewData interview, IPublishedEvent<VariablesChanged> @event)
        {
            foreach (var changedVariable in @event.Payload.ChangedVariables)
            {
                PreformActionOnLevel(interview, changedVariable.Identity.RosterVector, (interviewLevel) =>
                {
                    interviewLevel.Variables[changedVariable.Identity.Id] = changedVariable.NewValue;
                });
            }
            return interview;
        }

        public InterviewData Update(InterviewData interview, IPublishedEvent<VariablesDisabled> @event)
        {
            foreach (var disabledVariable in @event.Payload.Variables)
            {
                PreformActionOnLevel(interview, disabledVariable.RosterVector, (interviewLevel) =>
                {
                    interviewLevel.DisabledVariables.Add(disabledVariable.Id);
                });
            }
            return interview;
        }

        public InterviewData Update(InterviewData interview, IPublishedEvent<VariablesEnabled> @event)
        {
            foreach (var enabledVariable in @event.Payload.Variables)
            {
                PreformActionOnLevel(interview, enabledVariable.RosterVector, (interviewLevel) =>
                {
                    interviewLevel.DisabledVariables.Remove(enabledVariable.Id);
                });
            }
            return interview;
        }

        public InterviewData Update(InterviewData interview, IPublishedEvent<TranslationSwitched> @event)
        {
            interview.CurrentLanguage = @event.Payload.Language;
            return interview;
        }
    }
}
