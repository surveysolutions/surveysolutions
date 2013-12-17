using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

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
        IUpdateHandler<ViewWithSequence<InterviewData>, RosterTitleChanged>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswerCommented>,
        IUpdateHandler<ViewWithSequence<InterviewData>, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, NumericRealQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, NumericQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, NumericIntegerQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, TextQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, SingleOptionQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, SingleOptionLinkedQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, DateTimeQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GeoLocationQuestionAnswered>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswerRemoved>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GroupDisabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, GroupEnabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, QuestionDisabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, QuestionEnabled>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswerDeclaredInvalid>,
        IUpdateHandler<ViewWithSequence<InterviewData>, AnswerDeclaredValid>,
        IUpdateHandler<ViewWithSequence<InterviewData>, FlagRemovedFromAnswer>,
        IUpdateHandler<ViewWithSequence<InterviewData>, FlagSetToAnswer>,
        IUpdateHandler<ViewWithSequence<InterviewData>, InterviewDeclaredInvalid>,
        IUpdateHandler<ViewWithSequence<InterviewData>, InterviewDeclaredValid>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnriePropagationStructures;


        public override Type[] UsesViews
        {
            get { return new Type[] { typeof(UserDocument), typeof(QuestionnaireRosterStructure) }; }
        }

        private string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            if (vector.Length == 0)
                return "#";
            return string.Join(",", vector);
        }

        private Guid GetScopeOfPassedGroup(InterviewData interview, Guid groupId)
        {
            var questionnarie = questionnriePropagationStructures.GetById(interview.QuestionnaireId, interview.QuestionnaireVersion);

            foreach (var scopeId in questionnarie.RosterScopes.Keys)
            {
                if (questionnarie.RosterScopes[scopeId].RosterGroupsId.Contains(groupId))
                {
                    return scopeId;
                }
            }

            throw new ArgumentException(string.Format("group {0} is missing in any propagation scope of questionnaire",
                                                      groupId));
        }

        private void RemoveLevelsFromInterview(InterviewData interview, IEnumerable<string> levelKeysForDelete, Guid scopeId)
        {
            foreach (var levelKey in levelKeysForDelete)
            {
                RemoveLevelFromInterview(interview, levelKey, scopeId);
            }
        }

        private void AddNewLevelsToInterview(InterviewData interview, int startIndex, int count, decimal[] outerVector, int? sortIndex, Guid scopeId)
        {
            for (int rosterInstanceId = startIndex; rosterInstanceId < startIndex + count; rosterInstanceId++)
            {
                this.AddLevelToInterview(interview, outerVector, rosterInstanceId, sortIndex, scopeId);
            }
        }

        private void RemoveLevelFromInterview(InterviewData interview, string levelKey, Guid scopeId)
        {
            if (interview.Levels.ContainsKey(levelKey))
            {
                var level = interview.Levels[levelKey];
                if (!level.ScopeIds.ContainsKey(scopeId))
                    return;
                if (level.ScopeIds.Count == 1)
                    interview.Levels.Remove(levelKey);
                else
                    level.ScopeIds.Remove(scopeId);
            }
        }

        private void AddLevelToInterview(InterviewData interview, decimal[] vector, decimal rosterInstanceId, int? sortIndex, Guid scopeId)
        {
            var newVector = CreateNewVector(vector, rosterInstanceId);
            var levelKey = CreateLevelIdFromPropagationVector(newVector);
            if (!interview.Levels.ContainsKey(levelKey))
                interview.Levels[levelKey] = new InterviewLevel(scopeId, sortIndex, newVector);
            else
                interview.Levels[levelKey].ScopeIds[scopeId] = sortIndex;
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

        private InterviewData PreformActionOnLevel(InterviewData interview, decimal[] vector, Action<InterviewLevel> action)
        {
            var levelId = CreateLevelIdFromPropagationVector(vector);

            if (!interview.Levels.ContainsKey(levelId))
                return interview;

            action(interview.Levels[levelId]);
            return interview;
        }

        private InterviewData UpdateQuestion(InterviewData interview, decimal[] vector, Guid questionId, Action<InterviewQuestion> update)
        {
            return PreformActionOnLevel(interview, vector, (questionsAtTheLevel) =>
            {
                var answeredQuestion = questionsAtTheLevel.GetOrCreateQuestion(questionId);

                update(answeredQuestion);
            });
        }



        private InterviewData ChangeQuestionConditionState(InterviewData interview, decimal[] vector, Guid questionId, bool newState)
        {
            return this.UpdateQuestion(interview, vector, questionId, (question) =>
            {
                question.Enabled = newState;
            });
        }

        private InterviewData ChangeQuestionConditionValidity(InterviewData interview, decimal[] vector, Guid questionId, bool valid)
        {
            return this.UpdateQuestion(interview, vector, questionId, (question) =>
            {
                question.Valid = valid;
            });
        }

        private InterviewData SaveAnswer(InterviewData interview, decimal[] vector, Guid questionId, object answer)
         {
            return this.UpdateQuestion(interview, vector, questionId, (question) =>
                 {
                     question.Answer = answer;
                     question.IsAnswered = true;
                 });
         }

        private InterviewData SetFlagStateForQuestion(InterviewData interview, decimal[] vector, Guid questionId, bool isFlagged)
        {
            return this.UpdateQuestion(interview, vector, questionId, (question) =>
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

            return this.UpdateQuestion(interview, vector, questionId, (question) =>
            {
                if (question.Comments == null)
                    question.Comments = new List<InterviewQuestionComment>();
                question.Comments.Add(interviewQuestionComment);
            });
        }

        public InterviewEventHandlerFunctional(IReadSideRepositoryWriter<UserDocument> users, IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnriePropagationStructures,
            IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewData)
            : base(interviewData)
        {
            this.users = users;
            this.questionnriePropagationStructures = questionnriePropagationStructures;
        }

        public ViewWithSequence<InterviewData> Create(IPublishedEvent<InterviewCreated> evnt)
        {
            var responsible = this.users.GetById(evnt.Payload.UserId);

            var interview = new InterviewData()
            {
                InterviewId = evnt.EventSourceId,
                UpdateDate = evnt.EventTimeStamp,
                QuestionnaireId = evnt.Payload.QuestionnaireId,
                QuestionnaireVersion = evnt.Payload.QuestionnaireVersion,
                ResponsibleId = evnt.Payload.UserId, // Creator is responsible
                ResponsibleRole = responsible.Roles.FirstOrDefault()
            };
            var emptyVector = new decimal[0];
            interview.Levels.Add(CreateLevelIdFromPropagationVector(emptyVector), new InterviewLevel(evnt.EventSourceId, null, emptyVector));
            return new ViewWithSequence<InterviewData>(interview, evnt.EventSequence);
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



        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<RosterRowAdded> evnt)
        {
            Guid scopeOfCurrentGroup = GetScopeOfPassedGroup(currentState.Document,
                                                          evnt.Payload.GroupId);

            this.AddLevelToInterview(currentState.Document, evnt.Payload.OuterRosterVector, evnt.Payload.RosterInstanceId, evnt.Payload.SortIndex, scopeOfCurrentGroup);

            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<RosterRowRemoved> evnt)
        {
            Guid scopeOfCurrentGroup = GetScopeOfPassedGroup(currentState.Document,
                                                         evnt.Payload.GroupId);

            var newVector = CreateNewVector(evnt.Payload.OuterRosterVector, evnt.Payload.RosterInstanceId);
            var levelKey = CreateLevelIdFromPropagationVector(newVector);
            this.RemoveLevelFromInterview(currentState.Document, levelKey, scopeOfCurrentGroup);

            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<GroupPropagated> evnt)
        {
            Guid scopeOfCurrentGroup = GetScopeOfPassedGroup(currentState.Document,
                                                          evnt.Payload.GroupId);
            List<string> keysOfLevelsByScope =
                GetLevelsByScopeFromInterview(interview: currentState.Document, scopeId: scopeOfCurrentGroup);

            int countOfLevelByScope = keysOfLevelsByScope.Count();

            if (evnt.Payload.Count == countOfLevelByScope)
                return currentState;

            if (countOfLevelByScope < evnt.Payload.Count)
            {
                AddNewLevelsToInterview(currentState.Document, startIndex: countOfLevelByScope,
                    count: evnt.Payload.Count - countOfLevelByScope,
                    outerVector: evnt.Payload.OuterScopePropagationVector, sortIndex: null, scopeId: scopeOfCurrentGroup);
            }
            else
            {
                var keysOfLevelToBeDeleted =
                    keysOfLevelsByScope.Skip(evnt.Payload.Count).Take(countOfLevelByScope - evnt.Payload.Count);
                RemoveLevelsFromInterview(currentState.Document, keysOfLevelToBeDeleted, scopeOfCurrentGroup);
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

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<AnswerRemoved> evnt)
        {
            return
                new ViewWithSequence<InterviewData>(
                    this.UpdateQuestion(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, question =>
                    {
                        question.Answer = null;
                        question.IsAnswered = false;
                    }), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<GroupDisabled> evnt)
        {
            return
                new ViewWithSequence<InterviewData>(PreformActionOnLevel(currentState.Document, evnt.Payload.PropagationVector, (level) =>
                {
                    if (!level.DisabledGroups.Contains(evnt.Payload.GroupId))
                    {
                        level.DisabledGroups.Add(evnt.Payload.GroupId);
                    }
                }), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<GroupEnabled> evnt)
        {
            return
                new ViewWithSequence<InterviewData>(PreformActionOnLevel(currentState.Document, evnt.Payload.PropagationVector, (level) =>
            {
                if (level.DisabledGroups.Contains(evnt.Payload.GroupId))
                {
                    level.DisabledGroups.Remove(evnt.Payload.GroupId);
                }
            }),evnt.EventSequence);
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

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<AnswerDeclaredInvalid> evnt)
        {
            return
                new ViewWithSequence<InterviewData>(ChangeQuestionConditionValidity(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
               false), evnt.EventSequence);
        }

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<AnswerDeclaredValid> evnt)
        {
            return
                new ViewWithSequence<InterviewData>(
                    ChangeQuestionConditionValidity(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                        true), evnt.EventSequence);
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

        public ViewWithSequence<InterviewData> Update(ViewWithSequence<InterviewData> currentState, IPublishedEvent<RosterTitleChanged> evnt)
        {
            throw new NotImplementedException();

            /*return
                new ViewWithSequence<InterviewData>(
                    PreformActionOnLevel(currentState.Document, evnt.Payload.PropagationVector, (level) =>
                {
                    if (!level.DisabledGroups.Contains(evnt.Payload.GroupId))
                    {
                        level.DisabledGroups.Add(evnt.Payload.GroupId);
                    }
                }), evnt.EventSequence);*/
        }
    }
}
