using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewDenormalizerFunctional : FunctionalDenormalizer<IntervieweWithSequence>, ICreateHandler<IntervieweWithSequence, InterviewCreated>,
        IUpdateHandler<IntervieweWithSequence, InterviewStatusChanged>,
        IUpdateHandler<IntervieweWithSequence, SupervisorAssigned>,
        IUpdateHandler<IntervieweWithSequence, InterviewerAssigned>,
        IUpdateHandler<IntervieweWithSequence, GroupPropagated>,
        IUpdateHandler<IntervieweWithSequence, AnswerCommented>,
        IUpdateHandler<IntervieweWithSequence, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<IntervieweWithSequence, NumericRealQuestionAnswered>,
        IUpdateHandler<IntervieweWithSequence, NumericQuestionAnswered>,
        IUpdateHandler<IntervieweWithSequence, NumericIntegerQuestionAnswered>,
        IUpdateHandler<IntervieweWithSequence, TextQuestionAnswered>,
        IUpdateHandler<IntervieweWithSequence, SingleOptionQuestionAnswered>,
        IUpdateHandler<IntervieweWithSequence, SingleOptionLinkedQuestionAnswered>,
        IUpdateHandler<IntervieweWithSequence, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateHandler<IntervieweWithSequence, DateTimeQuestionAnswered>,
        IUpdateHandler<IntervieweWithSequence, GeoLocationQuestionAnswered>,
        IUpdateHandler<IntervieweWithSequence, AnswerRemoved>,
        IUpdateHandler<IntervieweWithSequence, GroupDisabled>,
        IUpdateHandler<IntervieweWithSequence, GroupEnabled>,
        IUpdateHandler<IntervieweWithSequence, QuestionDisabled>,
        IUpdateHandler<IntervieweWithSequence, QuestionEnabled>,
        IUpdateHandler<IntervieweWithSequence, AnswerDeclaredInvalid>,
        IUpdateHandler<IntervieweWithSequence, AnswerDeclaredValid>,
        IUpdateHandler<IntervieweWithSequence, FlagRemovedFromAnswer>,
        IUpdateHandler<IntervieweWithSequence, FlagSetToAnswer>,
        IUpdateHandler<IntervieweWithSequence, InterviewDeclaredInvalid>,
        IUpdateHandler<IntervieweWithSequence, InterviewDeclaredValid>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnriePropagationStructures;


        public override Type[] UsesViews
        {
            get { return new Type[] { typeof(UserDocument), typeof(QuestionnaireRosterStructure) }; }
        }

        private string CreateLevelIdFromPropagationVector(int[] vector)
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
                if (questionnarie.RosterScopes[scopeId].Contains(groupId))
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

        private void AddNewLevelsToInterview(InterviewData interview, int startIndex, int count, int[] outerVector, Guid scopeId)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {
                AddLevelToInterview(interview, outerVector, i, scopeId);
            }
        }

        private void RemoveLevelFromInterview(InterviewData interview, string levelKey, Guid scopeId)
        {
            if (interview.Levels.ContainsKey(levelKey))
            {
                var level = interview.Levels[levelKey];
                if (!level.ScopeIds.Contains(scopeId))
                    return;
                if (level.ScopeIds.Count == 1)
                    interview.Levels.Remove(levelKey);
                else
                    level.ScopeIds.Remove(scopeId);
            }
        }

        private void AddLevelToInterview(InterviewData interview, int[] vector, int index, Guid scopeId)
        {
            var newVector = CreateNewVector(vector, index);
            var levelKey = CreateLevelIdFromPropagationVector(newVector);
            if (!interview.Levels.ContainsKey(levelKey))
                interview.Levels[levelKey] = new InterviewLevel(scopeId, newVector);
            else
            {
                interview.Levels[levelKey].ScopeIds.Add(scopeId);
            }
        }

        private int[] CreateNewVector(int[] outerScopePropagationVector, int indexInScope)
        {
            var scopeVecor = new int[outerScopePropagationVector.Length + 1];
            outerScopePropagationVector.CopyTo(scopeVecor, 0);
            scopeVecor[scopeVecor.Length - 1] = indexInScope;
            return scopeVecor;
        }

        private List<string> GetLevelsByScopeFromInterview(InterviewData interview, Guid scopeId)
        {
            return interview.Levels.Where(level => level.Value.ScopeIds.Contains(scopeId))
                            .Select(level => level.Key).ToList();
        }

        private InterviewData PreformActionOnLevel(InterviewData interview, int[] vector, Action<InterviewLevel> action)
        {
            var levelId = CreateLevelIdFromPropagationVector(vector);

            if (!interview.Levels.ContainsKey(levelId))
                return interview;

            action(interview.Levels[levelId]);
            return interview;
        }

        private InterviewData UpdateQuestion(InterviewData interview, int[] vector, Guid questionId, Action<InterviewQuestion> update)
        {
            return PreformActionOnLevel(interview, vector, (questionsAtTheLevel) =>
            {
                var answeredQuestion = questionsAtTheLevel.GetOrCreateQuestion(questionId);

                update(answeredQuestion);
            });
        }



        private InterviewData ChangeQuestionConditionState(InterviewData interview, int[] vector, Guid questionId, bool newState)
        {
            return this.UpdateQuestion(interview, vector, questionId, (question) =>
            {
                question.Enabled = newState;
            });
        }

        private InterviewData ChangeQuestionConditionValidity(InterviewData interview, int[] vector, Guid questionId, bool valid)
        {
            return this.UpdateQuestion(interview, vector, questionId, (question) =>
            {
                question.Valid = valid;
            });
        }

        private InterviewData SaveAnswer(InterviewData interview, int[] vector, Guid questionId, object answer)
         {
            return this.UpdateQuestion(interview, vector, questionId, (question) =>
                 {
                     question.Answer = answer;
                     question.IsAnswered = true;
                 });
         }

        private InterviewData SetFlagStateForQuestion(InterviewData interview, int[] vector, Guid questionId, bool isFlagged)
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

        private InterviewData SaveComment(InterviewData interview, int[] vector, Guid questionId, string comment, Guid userId, string userName,
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

        public InterviewDenormalizerFunctional(IReadSideRepositoryWriter<UserDocument> users, IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnriePropagationStructures,
            IStorageStrategy<IntervieweWithSequence> storageStrategy)
            : base(storageStrategy)
        {
            this.users = users;
            this.questionnriePropagationStructures = questionnriePropagationStructures;
        }

        public IntervieweWithSequence Create(IPublishedEvent<InterviewCreated> evnt)
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
            var emptyVector = new int[0];
            interview.Levels.Add(CreateLevelIdFromPropagationVector(emptyVector), new InterviewLevel(evnt.EventSourceId, emptyVector));
            return new IntervieweWithSequence(interview, evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<InterviewStatusChanged> evnt)
        {
            currentState.Document.Status = evnt.Payload.Status;

            if (!currentState.Document.WasCompleted && evnt.Payload.Status == InterviewStatus.Completed)
            {
                currentState.Document.WasCompleted = true;
            }
            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<SupervisorAssigned> evnt)
        {
            currentState.Document.ResponsibleId = evnt.Payload.SupervisorId;
            currentState.Document.ResponsibleRole = UserRoles.Supervisor;
            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<InterviewerAssigned> evnt)
        {
            currentState.Document.ResponsibleId = evnt.Payload.InterviewerId;
            currentState.Document.ResponsibleRole = UserRoles.Operator;
            currentState.Sequence = evnt.EventSequence;
            return currentState;
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<GroupPropagated> evnt)
        {
            Guid scopeOfCurrentGroup = GetScopeOfPassedGroup(currentState.Document,
                                                          evnt.Payload.GroupId);
            /*if (scopeOfCurrentGroup == null)
                return;*/

            List<string> keysOfLevelsByScope =
                GetLevelsByScopeFromInterview(interview: currentState.Document, scopeId: scopeOfCurrentGroup);

            int countOfLevelByScope = keysOfLevelsByScope.Count();

            if (evnt.Payload.Count == countOfLevelByScope)
                return currentState;

            if (countOfLevelByScope < evnt.Payload.Count)
            {
                AddNewLevelsToInterview(currentState.Document, startIndex: countOfLevelByScope,
                             count: evnt.Payload.Count - countOfLevelByScope,
                             outerVector: evnt.Payload.OuterScopePropagationVector, scopeId: scopeOfCurrentGroup);
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

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<AnswerCommented> evnt)
        {
            var commenter = this.users.GetById(evnt.Payload.UserId);
            if (commenter == null)
                return currentState;

            return new IntervieweWithSequence(SaveComment(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.Comment, evnt.Payload.UserId, commenter.UserName, evnt.Payload.CommentTime), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            return new IntervieweWithSequence(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.SelectedValues), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            return new IntervieweWithSequence(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                evnt.Payload.Answer), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            return new IntervieweWithSequence(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.Answer), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            return new IntervieweWithSequence(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.Answer), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<TextQuestionAnswered> evnt)
        {
            return new IntervieweWithSequence(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.Answer), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            return new IntervieweWithSequence(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.SelectedValue), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            return new IntervieweWithSequence(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.SelectedPropagationVector), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            return new IntervieweWithSequence(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.SelectedPropagationVectors), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            return new IntervieweWithSequence(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.Answer), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            return new IntervieweWithSequence(SaveAnswer(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy, evnt.Payload.Timestamp)), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<AnswerRemoved> evnt)
        {
            return
                new IntervieweWithSequence(
                    this.UpdateQuestion(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, question =>
                    {
                        question.Answer = null;
                        question.IsAnswered = false;
                    }), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<GroupDisabled> evnt)
        {
            return
                new IntervieweWithSequence(PreformActionOnLevel(currentState.Document, evnt.Payload.PropagationVector, (level) =>
                {
                    if (!level.DisabledGroups.Contains(evnt.Payload.GroupId))
                    {
                        level.DisabledGroups.Add(evnt.Payload.GroupId);
                    }
                }), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<GroupEnabled> evnt)
        {
            return
                new IntervieweWithSequence(PreformActionOnLevel(currentState.Document, evnt.Payload.PropagationVector, (level) =>
            {
                if (level.DisabledGroups.Contains(evnt.Payload.GroupId))
                {
                    level.DisabledGroups.Remove(evnt.Payload.GroupId);
                }
            }),evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<QuestionDisabled> evnt)
        {
            return
                new IntervieweWithSequence(
                    ChangeQuestionConditionState(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                        false), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<QuestionEnabled> evnt)
        {
            return
                new IntervieweWithSequence(
                    ChangeQuestionConditionState(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
               true), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<AnswerDeclaredInvalid> evnt)
        {
            return
                new IntervieweWithSequence(ChangeQuestionConditionValidity(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
               false), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<AnswerDeclaredValid> evnt)
        {
            return
                new IntervieweWithSequence(
                    ChangeQuestionConditionValidity(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                        true), evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<FlagRemovedFromAnswer> evnt)
        {
            return
                new IntervieweWithSequence(
                    ChangeQuestionConditionValidity(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, false),
                    evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<FlagSetToAnswer> evnt)
        {
            return
                new IntervieweWithSequence(
                    SetFlagStateForQuestion(currentState.Document, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, true),
                    evnt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<InterviewDeclaredInvalid> evt)
        {
            return new IntervieweWithSequence(this.SetInterviewValidity(currentState.Document, false), evt.EventSequence);
        }

        public IntervieweWithSequence Update(IntervieweWithSequence currentState, IPublishedEvent<InterviewDeclaredValid> evt)
        {
            return new IntervieweWithSequence(this.SetInterviewValidity(currentState.Document, true), evt.EventSequence);
        }
    }
}
