using System;
using System.Collections.Generic;
using System.Globalization;
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
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewSummaryEventHandlerFunctional : AbstractFunctionalEventHandler<InterviewSummary>, 
        ICreateHandler<InterviewSummary, InterviewCreated>,
        IUpdateHandler<InterviewSummary, InterviewStatusChanged>,
        IUpdateHandler<InterviewSummary, SupervisorAssigned>,
        IUpdateHandler<InterviewSummary, TextQuestionAnswered>,
        IUpdateHandler<InterviewSummary, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<InterviewSummary, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewSummary, DateTimeQuestionAnswered>,
        IUpdateHandler<InterviewSummary, GeoLocationQuestionAnswered>,
        IUpdateHandler<InterviewSummary, AnswerRemoved>,
        IUpdateHandler<InterviewSummary, InterviewerAssigned>,
        IUpdateHandler<InterviewSummary, InterviewDeleted>,
        IUpdateHandler<InterviewSummary, InterviewRestored>,
        IUpdateHandler<InterviewSummary, InterviewDeclaredInvalid>,
        IUpdateHandler<InterviewSummary, InterviewDeclaredValid>
    {
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires;
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        public override Type[] UsesViews
        {
            get { return new Type[] { typeof(UserDocument), typeof(QuestionnaireBrowseItem) }; }
        }

        public InterviewSummaryEventHandlerFunctional(IReadSideRepositoryWriter<InterviewSummary> interviewSummary,
            IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires, IReadSideRepositoryWriter<UserDocument> users)
            : base(interviewSummary)
        {
            this.questionnaires = questionnaires;
            this.users = users;
        }


        private InterviewSummary UpdateInterviewSummary(InterviewSummary interviewSummary, DateTime updateDateTime, Action<InterviewSummary> update)
        {
            update(interviewSummary);
            interviewSummary.UpdateDate = updateDateTime;
            return interviewSummary;
        }

        private InterviewSummary AnswerQuestion(InterviewSummary interviewSummary, Guid questionId, string answer, DateTime updateDate)
        {
           return this.UpdateInterviewSummary(interviewSummary, updateDate, interview =>
            {
                if (interview.AnswersToFeaturedQuestions.ContainsKey(questionId))
                {
                    interview.AnswersToFeaturedQuestions[questionId].Answer = answer;
                }
            });
        }

        public InterviewSummary Create(IPublishedEvent<InterviewCreated> evnt)
        {
            UserDocument responsible = this.users.GetById(evnt.Payload.UserId);
            return 
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
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewStatusChanged> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
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

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<SupervisorAssigned> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                var supervisorName = this.users.GetById(evnt.Payload.SupervisorId).UserName;

                interview.ResponsibleId = evnt.Payload.SupervisorId;
                interview.ResponsibleName = supervisorName;
                interview.ResponsibleRole = UserRoles.Supervisor;
                interview.TeamLeadId = evnt.Payload.SupervisorId;
                interview.TeamLeadName = supervisorName;
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<TextQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId,
                              string.Join(",", evnt.Payload.SelectedValues.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray()), evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, evnt.Payload.SelectedValue.ToString(CultureInfo.InvariantCulture), evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, evnt.Payload.Answer.ToString(CultureInfo.InvariantCulture), evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, evnt.Payload.Answer.ToString(CultureInfo.InvariantCulture), evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, evnt.Payload.Answer.ToString(CultureInfo.InvariantCulture), evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, evnt.Payload.Answer.ToString("d", CultureInfo.InvariantCulture), evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId,
                            string.Format("{0},{1}[{2}]", evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy), evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<AnswerRemoved> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                if (interview.AnswersToFeaturedQuestions.ContainsKey(evnt.Payload.QuestionId))
                {
                    interview.AnswersToFeaturedQuestions[evnt.Payload.QuestionId].Answer = string.Empty;
                }
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewerAssigned> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                var interviewerName = this.users.GetById(evnt.Payload.InterviewerId).UserName;

                interview.ResponsibleId = evnt.Payload.InterviewerId;
                interview.ResponsibleName = interviewerName;
                interview.ResponsibleRole = UserRoles.Operator;
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewDeleted> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                interview.IsDeleted = true;
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewRestored> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                interview.IsDeleted = false;
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewDeclaredInvalid> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                interview.HasErrors = true;
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewDeclaredValid> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                interview.HasErrors = false;
            });
        }
    }
}
