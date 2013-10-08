using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    internal class InterviewDenormalizer : IEventHandler,
                                         IEventHandler<InterviewCreated>,
                                         IEventHandler<InterviewStatusChanged>,
                                         IEventHandler<SupervisorAssigned>,
                                         IEventHandler<InterviewerAssigned>,

        IEventHandler<GroupPropagated>,
        IEventHandler<AnswerCommented>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<GroupDisabled>,
        IEventHandler<GroupEnabled>,
        IEventHandler<QuestionDisabled>,
        IEventHandler<QuestionEnabled>,
        IEventHandler<AnswerDeclaredInvalid>,
        IEventHandler<AnswerDeclaredValid>,
        IEventHandler<FlagRemovedFromAnswer>,
        IEventHandler<FlagSetToAnswer>,
        IEventHandler<InterviewDeclaredInvalid>,
        IEventHandler<InterviewDeclaredValid>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure> questionnriePropagationStructures;
        private readonly IReadSideRepositoryWriter<InterviewData> interviews;

        public InterviewDenormalizer(IReadSideRepositoryWriter<UserDocument> users, 
                                     IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure> questionnriePropagationStructures, 
                                     IReadSideRepositoryWriter<InterviewData> interviews)
        {
            this.users = users;
            this.questionnriePropagationStructures = questionnriePropagationStructures;
            this.interviews = interviews;
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new[] { typeof(UserDocument), typeof(QuestionnairePropagationStructure) }; }
        }

        public Type[] BuildsViews
        {
            get { return new[] { typeof(InterviewData) }; }
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
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
            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var interview = this.interviews.GetById(evnt.EventSourceId);

            interview.Status = evnt.Payload.Status;

            if (!interview.WasCompleted && evnt.Payload.Status == InterviewStatus.Completed)
            {
                interview.WasCompleted = true;
            }

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            var interview = this.interviews.GetById(evnt.EventSourceId);

            interview.ResponsibleId = evnt.Payload.SupervisorId;
            interview.ResponsibleRole = UserRoles.Supervisor;

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<GroupPropagated> evnt)
        {
            InterviewData interview = this.interviews.GetById(evnt.EventSourceId);

            Guid scopeOfCurrentGroup = GetScopeOfPassedGroup(interview,
                                                            evnt.Payload.GroupId);
            /*if (scopeOfCurrentGroup == null)
                return;*/

            List<string> keysOfLevelsByScope =
                GetLevelsByScopeFromInterview(interview: interview, scopeId: scopeOfCurrentGroup);

            int countOfLevelByScope = keysOfLevelsByScope.Count();

            if (evnt.Payload.Count == countOfLevelByScope)
                return;

            if (countOfLevelByScope < evnt.Payload.Count)
            {
                AddNewLevelsToInterview(interview, startIndex: countOfLevelByScope,
                             count: evnt.Payload.Count - countOfLevelByScope,
                             outerVector: evnt.Payload.OuterScopePropagationVector, scopeId: scopeOfCurrentGroup);
            }
            else
            {
                var keysOfLevelToBeDeleted =
                    keysOfLevelsByScope.Skip(evnt.Payload.Count).Take(countOfLevelByScope - evnt.Payload.Count);
                RemoveLevelsFromInterview(interview, keysOfLevelToBeDeleted, scopeOfCurrentGroup);
            }

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<AnswerCommented> evnt)
        {

            var commenter = this.users.GetById(evnt.Payload.UserId);
            if (commenter == null)
                return;
            SaveComment(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                        evnt.Payload.Comment, evnt.Payload.UserId, commenter.UserName, evnt.Payload.CommentTime);

        }

        public void Handle(IPublishedEvent<FlagRemovedFromAnswer> evnt)
        {
            SetFlagStateForQuestion(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, false);
        }

        public void Handle(IPublishedEvent<FlagSetToAnswer> evnt)
        {
            SetFlagStateForQuestion(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, true);
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            SaveAnswer(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                       evnt.Payload.SelectedValues);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            SaveAnswer(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                       evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            SaveAnswer(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                    evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            SaveAnswer(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                        evnt.Payload.SelectedValue);
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            SaveAnswer(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                     evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            SaveAnswer(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                       new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy, evnt.Payload.Timestamp));
        }

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            SaveAnswer(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, evnt.Payload.SelectedPropagationVector);
        }

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            SaveAnswer(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId, evnt.Payload.SelectedPropagationVectors);
        }

        public void Handle(IPublishedEvent<GroupDisabled> evnt)
        {
            PreformActionOnLevel(evnt.EventSourceId, evnt.Payload.PropagationVector, (level) =>
                {
                    if (!level.DisabledGroups.Contains(evnt.Payload.GroupId))
                    {
                        level.DisabledGroups.Add(evnt.Payload.GroupId);
                        return true;
                    }
                    return false;
                });
        }

        public void Handle(IPublishedEvent<GroupEnabled> evnt)
        {
            PreformActionOnLevel(evnt.EventSourceId, evnt.Payload.PropagationVector, (level) =>
                {
                    if (level.DisabledGroups.Contains(evnt.Payload.GroupId))
                    {
                        level.DisabledGroups.Remove(evnt.Payload.GroupId);
                        return true;
                    }
                    return false;
                });
        }

        public void Handle(IPublishedEvent<QuestionDisabled> evnt)
        {
            ChangeQuestionConditionState(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                 false);
        }

        public void Handle(IPublishedEvent<QuestionEnabled> evnt)
        {
            ChangeQuestionConditionState(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                 true);
        }

        public void Handle(IPublishedEvent<AnswerDeclaredInvalid> evnt)
        {
            ChangeQuestionConditionValidity(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
               false);
        }

        public void Handle(IPublishedEvent<AnswerDeclaredValid> evnt)
        {
            ChangeQuestionConditionValidity(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
               true);
        }

        private string CreateLevelIdFromPropagationVector(int[] vector)
        {
            if (vector.Length == 0)
                return "#";
            return string.Join(",", vector);
        }

        private void SaveAnswer(Guid interviewId, int[] vector, Guid questionId, object answer)
        {
            PreformActionOnQuestion(interviewId, vector, questionId, (question) =>
                {
                    question.Answer = answer;
                    question.IsAnswered = true;
                    return true;
                });
        }

        private void SetFlagStateForQuestion(Guid interviewId, int[] vector, Guid questionId, bool isFlagged)
        {
            PreformActionOnQuestion(interviewId, vector, questionId, (question) =>
            {
                if (question.IsFlagged == isFlagged)
                    return false;
                question.IsFlagged = isFlagged;
                return true;
            });
        }

        private void SaveComment(Guid interviewId, int[] vector, Guid questionId, string comment, Guid userId, string userName, DateTime commentTime)
        {
            var interviewQuestionComment = new InterviewQuestionComment()
                {
                    Id = Guid.NewGuid(),
                    Text = comment,
                    CommenterId = userId,
                    CommenterName = userName,
                    Date = commentTime
                };

            PreformActionOnQuestion(interviewId, vector, questionId, (question) =>
                {
                    if (question.Comments == null)
                        question.Comments = new List<InterviewQuestionComment>();
                    question.Comments.Add(interviewQuestionComment);
                    return true;
                });
        }

        private void ChangeQuestionConditionState(Guid interviewId, int[] vector, Guid questionId, bool newState)
        {
            PreformActionOnQuestion(interviewId, vector, questionId, (question) =>
                {
                    if (question.Enabled == newState)
                        return false;
                    question.Enabled = newState;
                    return true;
                });
        }

        private void ChangeQuestionConditionValidity(Guid interviewId, int[] vector, Guid questionId, bool valid)
        {
            PreformActionOnQuestion(interviewId, vector, questionId, (question) =>
                {
                    if (question.Valid == valid)
                        return false;
                    question.Valid = valid;
                    return true;
                });
        }

        private void PreformActionOnLevel(Guid interviewId, int[] vector, Func<InterviewLevel, bool> action)
        {
            var interview = this.interviews.GetById(interviewId);
            var levelId = CreateLevelIdFromPropagationVector(vector);
            if (action(interview.Levels[levelId]))
            {
                this.interviews.Store(interview, interview.InterviewId);
            }
        }


        private void PreformActionOnQuestion(Guid interviewId, int[] vector, Guid questionId,
                                             Func<InterviewQuestion, bool> action)
        {
            PreformActionOnLevel(interviewId, vector, (questionsAtTheLevel) =>
                {
                    var answeredQuestion = questionsAtTheLevel.Questions.FirstOrDefault(q => q.Id == questionId);
                    if (answeredQuestion == null)
                    {
                        answeredQuestion = new InterviewQuestion(questionId);
                        questionsAtTheLevel.Questions.Add(answeredQuestion);
                    }
                   
                    return action(answeredQuestion);
                });
        }

        private void RemoveLevelFromInterview(InterviewData interview, string levelKey, Guid scopeId)
        {
            if (interview.Levels.ContainsKey(levelKey))
            {
                var level = interview.Levels[levelKey];
                if(!level.ScopeIds.Contains(scopeId))
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
            if(!interview.Levels.ContainsKey(levelKey))
                interview.Levels[levelKey]= new InterviewLevel(scopeId, newVector);
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

        private Guid GetScopeOfPassedGroup(InterviewData interview, Guid groupId)
        {
            var questionnarie = questionnriePropagationStructures.GetById(interview.QuestionnaireId, interview.QuestionnaireVersion);

            foreach (var scopeId in questionnarie.PropagationScopes.Keys)
            {
                if (questionnarie.PropagationScopes[scopeId].Contains(groupId))
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
            for (int i = startIndex; i < startIndex + count; i ++)
            {
                AddLevelToInterview(interview, outerVector, i, scopeId);
            }
        }

        private List<string> GetLevelsByScopeFromInterview(InterviewData interview, Guid scopeId)
        {
            return interview.Levels.Where(level => level.Value.ScopeIds.Contains(scopeId))
                            .Select(level => level.Key).ToList();
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            var interview = this.interviews.GetById(evnt.EventSourceId);

            interview.ResponsibleId = evnt.Payload.InterviewerId;
            interview.ResponsibleRole = UserRoles.Operator;

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<InterviewDeclaredInvalid> evnt)
        {
            this.SetInterviewValidity(evnt.EventSourceId, false);
        }

        public void Handle(IPublishedEvent<InterviewDeclaredValid> evnt)
        {
            this.SetInterviewValidity(evnt.EventSourceId, true);
        }

        private void SetInterviewValidity(Guid interviewId, bool isValid)
        {
            var interview = this.interviews.GetById(interviewId);

            interview.HasErrors = !isValid;

            this.interviews.Store(interview, interview.InterviewId);
        }
    }
}
