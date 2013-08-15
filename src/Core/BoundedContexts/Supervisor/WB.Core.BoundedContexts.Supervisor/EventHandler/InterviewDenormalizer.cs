using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewDenormalizer : IEventHandler,
                                         IEventHandler<InterviewCreated>,
                                         IEventHandler<InterviewStatusChanged>,
                                         IEventHandler<SupervisorAssigned>,

         IEventHandler<GroupPropagated>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<AnswerCommented>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<NumericQuestionAnswered>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GroupDisabled>,
        IEventHandler<GroupEnabled>,
        IEventHandler<QuestionDisabled>,
        IEventHandler<QuestionEnabled>,
        IEventHandler<AnswerDeclaredInvalid>,
        IEventHandler<AnswerDeclaredValid>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryWriter<QuestionnaireDocument> questionnries; 
        private readonly IReadSideRepositoryWriter<InterviewData> interviews;

        public InterviewDenormalizer(IReadSideRepositoryWriter<UserDocument> users, IReadSideRepositoryWriter<InterviewData> interviews, IReadSideRepositoryWriter<QuestionnaireDocument> questionnries)
        {
            this.users = users;
            this.interviews = interviews;
            this.questionnries = questionnries;
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
            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var interview = this.interviews.GetById(evnt.EventSourceId);

            interview.Status = evnt.Payload.Status;

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            var interview = this.interviews.GetById(evnt.EventSourceId);

            interview.ResponsibleId = evnt.Payload.SupervisorId;
            interview.ResponsibleRole = UserRoles.Supervisor;

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            var interview = this.interviews.GetById(evnt.EventSourceId);

            interview.Status = InterviewStatus.Completed;

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            var interview = this.interviews.GetById(evnt.EventSourceId);

            interview.Status = InterviewStatus.Restarted;

            this.interviews.Store(interview, interview.InterviewId);
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new[] { typeof(UserDocument), typeof(QuestionnaireDocument) }; }
        }
        public Type[] BuildsViews
        {
            get { return new [] { typeof(InterviewData) }; }
        }

        public void Handle(IPublishedEvent<GroupPropagated> evnt)
        {
            throw new NotImplementedException();
        }

        public void Handle(IPublishedEvent<AnswerCommented> evnt)
        {
            SaveComment(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                       evnt.Payload.Comment);
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            SaveAnswer(evnt.EventSourceId, evnt.Payload.PropagationVector, evnt.Payload.QuestionId,
                       evnt.Payload.SelectedValues);
        }

        public void Handle(IPublishedEvent<NumericQuestionAnswered> evnt)
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

        public void Handle(IPublishedEvent<GroupDisabled> evnt)
        {
            throw new NotImplementedException();
        }

        public void Handle(IPublishedEvent<GroupEnabled> evnt)
        {
            throw new NotImplementedException();
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
            return string.Join(",", vector);
        }

        private void SaveAnswer(Guid interviewId, int[] vector, Guid questionId, object answer)
        {
            PreformActionOnQuestion(interviewId, vector, questionId, (question) => { question.Answer = answer; });
        }

        private void SaveComment(Guid interviewId, int[] vector, Guid questionId, string comment)
        {
            PreformActionOnQuestion(interviewId, vector, questionId, (question) => { question.Comments = comment; });
        }

        private void ChangeQuestionConditionState(Guid interviewId, int[] vector, Guid questionId, bool newState)
        {
            PreformActionOnQuestion(interviewId, vector, questionId, (question) => { question.Enabled = newState; });
        }

        private void ChangeQuestionConditionValidity(Guid interviewId, int[] vector, Guid questionId, bool valid)
        {
            PreformActionOnQuestion(interviewId, vector, questionId, (question) => { question.Valid = valid; });
        }

        private void PreformActionOnQuestion(Guid interviewId, int[] vector, Guid questionId, Action<InterviewQuestion> action)
        {
            var interview = this.interviews.GetById(interviewId);
            var levelId = CreateLevelIdFromPropagationVector(vector);
            var questionsAtTheLevel = interview.Levels[levelId];
            var answeredQuestion = questionsAtTheLevel.First(q => q.Id == questionId);
            if (answeredQuestion == null)
            {
                answeredQuestion = new InterviewQuestion(questionId);
                questionsAtTheLevel.Add(answeredQuestion);
            }

            action(answeredQuestion);

            this.interviews.Store(interview, interview.InterviewId);
        }
    }
}
