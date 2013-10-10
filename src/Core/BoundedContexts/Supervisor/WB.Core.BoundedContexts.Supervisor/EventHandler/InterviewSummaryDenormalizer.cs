using System;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewSummaryDenormalizer :
        IEventHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewStatusChanged>,
        IEventHandler<SupervisorAssigned>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<NumericQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<InterviewerAssigned>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewRestored>,
        IEventHandler<InterviewDeclaredInvalid>,
        IEventHandler<InterviewDeclaredValid>
    {
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviews;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires;
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        public InterviewSummaryDenormalizer(IReadSideRepositoryWriter<UserDocument> users,
                                            IReadSideRepositoryWriter<InterviewSummary> interviews,
                                            IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires)
        {
            this.users = users;
            this.interviews = interviews;
            this.questionnaires = questionnaires;
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new[] {typeof (UserDocument), typeof (QuestionnaireBrowseItem)}; }
        }

        public Type[] BuildsViews
        {
            get { return new[] {typeof (InterviewSummary)}; }
        }

        private void UpdateInterviewSummary(Guid interviewId, DateTime updateDateTime, Action<InterviewSummary> update)
        {
            InterviewSummary interviewSummary = this.interviews.GetById(interviewId);

            update(interviewSummary);
            interviewSummary.UpdateDate = updateDateTime;

            this.interviews.Store(interviewSummary, interviewId);
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            UserDocument responsible = this.users.GetById(evnt.Payload.UserId);
            var interview =
                new InterviewSummary(this.questionnaires.GetById(evnt.Payload.QuestionnaireId,
                                                                 evnt.Payload.QuestionnaireVersion))
                    {
                        InterviewId = evnt.EventSourceId,
                        UpdateDate = evnt.EventTimeStamp,
                        QuestionnaireId = evnt.Payload.QuestionnaireId,
                        QuestionnaireVersion = evnt.Payload.QuestionnaireVersion,
                        QuestionnaireTitle =
                            questionnaires.GetById(
                                evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion).Title,
                        ResponsibleId = evnt.Payload.UserId, // Creator is responsible
                        ResponsibleName = this.users.GetById(evnt.Payload.UserId).UserName,
                        ResponsibleRole = responsible.Roles.FirstOrDefault()
                    };
            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            this.UpdateInterviewSummary(evnt.EventSourceId, evnt.EventTimeStamp, interview =>
            {
                interview.Status = evnt.Payload.Status;

                interview.CommentedStatusesHistory.Add(new InterviewCommentedStatus
                {
                    Status = interview.Status,
                    Date = interview.UpdateDate,
                    Comment = evnt.Payload.Comment
                });
            });
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            this.UpdateInterviewSummary(evnt.EventSourceId, evnt.EventTimeStamp, interview =>
            {
                var interviewerName = this.users.GetById(evnt.Payload.InterviewerId).UserName;

                interview.ResponsibleId = evnt.Payload.InterviewerId;
                interview.ResponsibleName = interviewerName;
                interview.ResponsibleRole = UserRoles.Operator;
            });
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            this.UpdateInterviewSummary(evnt.EventSourceId, evnt.EventTimeStamp, interview =>
            {
                var supervisorName = this.users.GetById(evnt.Payload.SupervisorId).UserName;

                interview.ResponsibleId = evnt.Payload.SupervisorId;
                interview.ResponsibleName = supervisorName;
                interview.ResponsibleRole = UserRoles.Supervisor;
                interview.TeamLeadId = evnt.Payload.SupervisorId;
                interview.TeamLeadName = supervisorName;
            });
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            this.UpdateInterviewSummary(evnt.EventSourceId, evnt.EventTimeStamp, interview =>
            {
                interview.IsDeleted = true;
            });
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer.ToString("d", CultureInfo.InvariantCulture), evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                                string.Join(",", evnt.Payload.SelectedValues.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray()), evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer.ToString(CultureInfo.InvariantCulture), evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                                evnt.Payload.SelectedValue.ToString(CultureInfo.InvariantCulture), evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, 
                                string.Format("{0},{1}[{2}]", evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy), 
                                evnt.EventTimeStamp);
        }

        private void AnswerQuestion(Guid interviewId, Guid questionId, string answer, DateTime updateDate)
        {
            this.UpdateInterviewSummary(interviewId, updateDate, interview =>
            {
                if (interview.AnswersToFeaturedQuestions.ContainsKey(questionId))
                {
                    interview.AnswersToFeaturedQuestions[questionId].Answer = answer;
                }
            });
        }
        
        public void Handle(IPublishedEvent<InterviewRestored> evnt)
        {
            this.UpdateInterviewSummary(evnt.EventSourceId, evnt.EventTimeStamp, interview =>
            {
                interview.IsDeleted = false;
            });
        }

        public void Handle(IPublishedEvent<InterviewDeclaredInvalid> evnt)
        {
            this.UpdateInterviewSummary(evnt.EventSourceId, evnt.EventTimeStamp, interview =>
            {
                interview.HasErrors = true;
            });
        }

        public void Handle(IPublishedEvent<InterviewDeclaredValid> evnt)
        {
            this.UpdateInterviewSummary(evnt.EventSourceId, evnt.EventTimeStamp, interview =>
            {
                interview.HasErrors = false;
            });
        }
    }
}