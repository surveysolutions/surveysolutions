using System;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class InterviewSummaryEventHandlerFunctional : AbstractFunctionalEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>,
        IUpdateHandler<InterviewSummary, InterviewCreated>,
        IUpdateHandler<InterviewSummary, InterviewFromPreloadedDataCreated>,
        IUpdateHandler<InterviewSummary, InterviewOnClientCreated>,
        IUpdateHandler<InterviewSummary, InterviewStatusChanged>,
        IUpdateHandler<InterviewSummary, SupervisorAssigned>,
        IUpdateHandler<InterviewSummary, TextQuestionAnswered>,
        IUpdateHandler<InterviewSummary, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<InterviewSummary, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewSummary, DateTimeQuestionAnswered>,
        IUpdateHandler<InterviewSummary, GeoLocationQuestionAnswered>,
        IUpdateHandler<InterviewSummary, QRBarcodeQuestionAnswered>,
        IUpdateHandler<InterviewSummary, AnswersRemoved>,
        IUpdateHandler<InterviewSummary, InterviewerAssigned>,
        IUpdateHandler<InterviewSummary, InterviewDeleted>,
        IUpdateHandler<InterviewSummary, InterviewRestored>,
        IUpdateHandler<InterviewSummary, InterviewRestarted>,
        IUpdateHandler<InterviewSummary, InterviewCompleted>,
        IUpdateHandler<InterviewSummary, InterviewRejected>,
        IUpdateHandler<InterviewSummary, InterviewApproved>,
        IUpdateHandler<InterviewSummary, InterviewRejectedByHQ>,
        IUpdateHandler<InterviewSummary, InterviewApprovedByHQ>,
        IUpdateHandler<InterviewSummary, InterviewDeclaredInvalid>,
        IUpdateHandler<InterviewSummary, InterviewDeclaredValid>,
        IUpdateHandler<InterviewSummary, SynchronizationMetadataApplied>,
        IUpdateHandler<InterviewSummary, InterviewHardDeleted>

    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaires;
        private readonly IReadSideRepositoryWriter<UserDocument> users;

        public InterviewSummaryEventHandlerFunctional(IReadSideRepositoryWriter<InterviewSummary> interviewSummary,
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaires, IReadSideRepositoryWriter<UserDocument> users)
            : base(interviewSummary)
        {
            this.questionnaires = questionnaires;
            this.users = users;
        }

        public override object[] Readers
        {
            get { return new object[] { questionnaires, users }; }
        }

        private InterviewSummary UpdateInterviewSummary(InterviewSummary interviewSummary, DateTime updateDateTime, Action<InterviewSummary> update)
        {
            update(interviewSummary);
            interviewSummary.UpdateDate = updateDateTime;
            return interviewSummary;
        }

        private InterviewSummary AnswerQuestion(InterviewSummary interviewSummary, Guid questionId, object answer, DateTime updateDate)
        {
           return this.UpdateInterviewSummary(interviewSummary, updateDate, interview =>
            {
                if (interview.AnswersToFeaturedQuestions.Any(x => x.Questionid == questionId))
                {
                    interview.AnswerFeaturedQuestion(questionId, AnswerUtils.AnswerToString(answer));
                }
            });
        }

        private InterviewSummary AnswerFeaturedQuestionWithOptions(InterviewSummary interviewSummary, Guid questionId, DateTime updateDate,
            params decimal[] answers)
        {
            return this.UpdateInterviewSummary(interviewSummary, updateDate, interview =>
            {
                if (interview.AnswersToFeaturedQuestions.Any(x => x.Questionid == questionId))
                {
                    var featuredQuestion = interview.AnswersToFeaturedQuestions.First(x => x.Questionid == questionId);
                    if (featuredQuestion == null)
                        return;

                    interview.AnswerFeaturedQuestion(questionId, answers);
                }
            });
        }

        private InterviewSummary CreateInterviewSummary(Guid userId, Guid questionnaireId, long questionnaireVersion,
            Guid eventSourceId, DateTime eventTimeStamp, bool wasCreatedOnClient)
        {
            UserDocument responsible = this.users.GetById(userId);
            var questionnarie = this.questionnaires.AsVersioned().Get(questionnaireId.FormatGuid(), questionnaireVersion);

            var interviewSummary = new InterviewSummary(questionnarie.Questionnaire)
            {
                InterviewId = eventSourceId,
                WasCreatedOnClient = wasCreatedOnClient,
                UpdateDate = eventTimeStamp,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                QuestionnaireTitle = questionnarie.Questionnaire.Title,
                ResponsibleId = userId, // Creator is responsible
                ResponsibleName = responsible != null ? responsible.UserName : "<UNKNOWN USER>",
                ResponsibleRole = responsible != null ? responsible.Roles.FirstOrDefault() : UserRoles.Undefined
            };
            AddInterviewStatus(summary: interviewSummary, status: InterviewStatus.Created, date: eventTimeStamp,
                comment: null, responsibleId: userId);

            return interviewSummary;
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewCreated> evnt)
        {
            return this.CreateInterviewSummary(evnt.Payload.UserId, evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion, evnt.EventSourceId, evnt.EventTimeStamp, wasCreatedOnClient: false);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            return this.CreateInterviewSummary(evnt.Payload.UserId, evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion, evnt.EventSourceId, evnt.EventTimeStamp, wasCreatedOnClient: false);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            return this.CreateInterviewSummary(evnt.Payload.UserId, evnt.Payload.QuestionnaireId,
             evnt.Payload.QuestionnaireVersion, evnt.EventSourceId, evnt.EventTimeStamp, wasCreatedOnClient: true);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewStatusChanged> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                interview.Status = evnt.Payload.Status;
                if (interview.CommentedStatusesHistory.Count <= 0) 
                    return;
                var lastHistoryStatus = interview.CommentedStatusesHistory.Last();
                if (lastHistoryStatus.Status != evnt.Payload.Status) 
                    return;
                lastHistoryStatus.Comment = evnt.Payload.Comment;
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<SupervisorAssigned> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                UserDocument userDocument = this.users.GetById(evnt.Payload.SupervisorId);
                var supervisorName = userDocument != null ? userDocument.UserName : "<UNKNOWN SUPERVISOR>";

                interview.ResponsibleId = evnt.Payload.SupervisorId;
                interview.ResponsibleName = supervisorName;
                interview.ResponsibleRole = UserRoles.Supervisor;
                interview.TeamLeadId = evnt.Payload.SupervisorId;
                interview.TeamLeadName = supervisorName;
                
                AddInterviewStatus(summary: interview, status: InterviewStatus.SupervisorAssigned,
                    date: evnt.EventTimeStamp, comment: null, responsibleId: evnt.Payload.UserId);
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<TextQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            return this.AnswerFeaturedQuestionWithOptions(currentState, evnt.Payload.QuestionId, evnt.EventTimeStamp, evnt.Payload.SelectedValues);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            return this.AnswerFeaturedQuestionWithOptions(currentState, evnt.Payload.QuestionId, evnt.EventTimeStamp,evnt.Payload.SelectedValue);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, evnt.Payload.Answer.ToString("u"), evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            var answerByGeoQuestion = new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy, evnt.Payload.Altitude, evnt.Payload.Timestamp);
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, answerByGeoQuestion, evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            return this.AnswerQuestion(currentState, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.EventTimeStamp);
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<AnswersRemoved> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                foreach (var question in evnt.Payload.Questions)
                {
                    if (interview.AnswersToFeaturedQuestions.Any(x => x.Questionid == question.Id))
                    {
                        interview.AnswerFeaturedQuestion(question.Id, string.Empty);
                    }
                }
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewerAssigned> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                var interviewerName = GetResponsibleIdName(evnt.Payload.InterviewerId);

                interview.ResponsibleId = evnt.Payload.InterviewerId;
                interview.ResponsibleName = interviewerName;
                interview.ResponsibleRole = UserRoles.Operator;

                AddInterviewStatus(summary: interview, status: InterviewStatus.InterviewerAssigned,
                    date: evnt.EventTimeStamp, comment: null, responsibleId: evnt.Payload.UserId);
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewDeleted> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                interview.IsDeleted = true;
                if (evnt.Origin != Constants.HeadquartersSynchronizationOrigin)
                {
                    AddInterviewStatus(summary: interview, status: InterviewStatus.Deleted,
                        date: evnt.EventTimeStamp, comment: null, responsibleId: evnt.Payload.UserId);
                }
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewHardDeleted> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                interview.IsDeleted = true;

                AddInterviewStatus(summary: interview, status: InterviewStatus.Deleted,
                    date: evnt.EventTimeStamp, comment: null, responsibleId: evnt.Payload.UserId);
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewRestored> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                interview.IsDeleted = false;

                if (evnt.Origin != Constants.HeadquartersSynchronizationOrigin)
                {
                    AddInterviewStatus(summary: interview, status: InterviewStatus.Restored,
                        date: evnt.EventTimeStamp, comment: null, responsibleId: evnt.Payload.UserId);
                }
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewRestarted> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                AddInterviewStatus(summary: interview, status: InterviewStatus.Restarted,
                    date: evnt.Payload.RestartTime ?? evnt.EventTimeStamp, comment: null, responsibleId: evnt.Payload.UserId);
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewCompleted> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                AddInterviewStatus(summary: interview, status: InterviewStatus.Completed,
                    date: evnt.Payload.CompleteTime ?? evnt.EventTimeStamp, comment: null, responsibleId: evnt.Payload.UserId);
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewRejected> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                AddInterviewStatus(summary: interview, status: InterviewStatus.RejectedBySupervisor,
                    date: evnt.EventTimeStamp, comment: evnt.Payload.Comment, responsibleId: evnt.Payload.UserId);
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewApproved> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                AddInterviewStatus(summary: interview, status: InterviewStatus.ApprovedBySupervisor,
                    date: evnt.EventTimeStamp, comment: evnt.Payload.Comment, responsibleId: evnt.Payload.UserId);
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                AddInterviewStatus(summary: interview, status: InterviewStatus.RejectedByHeadquarters,
                    date: evnt.EventTimeStamp, comment: evnt.Payload.Comment, responsibleId: evnt.Payload.UserId);
            });
        }

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                AddInterviewStatus(summary: interview, status: InterviewStatus.ApprovedByHeadquarters,
                    date: evnt.EventTimeStamp, comment: evnt.Payload.Comment, responsibleId: evnt.Payload.UserId);
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

        public InterviewSummary Update(InterviewSummary currentState, IPublishedEvent<SynchronizationMetadataApplied> evnt)
        {
            return this.UpdateInterviewSummary(currentState, evnt.EventTimeStamp, interview =>
            {
                if (currentState.WasCreatedOnClient)
                {
                    if (evnt.Payload.FeaturedQuestionsMeta != null)
                    {
                        foreach (var answeredQuestionSynchronizationDto in evnt.Payload.FeaturedQuestionsMeta)
                        {
                            if (interview.AnswersToFeaturedQuestions.Any(x => x.Questionid == answeredQuestionSynchronizationDto.Id))
                            {
                                interview.AnswerFeaturedQuestion(answeredQuestionSynchronizationDto.Id, answeredQuestionSynchronizationDto.Answer.ToString());
                            }
                        }
                    }
                    var responsible = this.users.GetById(currentState.ResponsibleId);
                    if (responsible != null && responsible.Supervisor != null)
                    {
                        currentState.TeamLeadId = responsible.Supervisor.Id;
                        currentState.TeamLeadName = responsible.Supervisor.Name;
                    }
                    currentState.Status = evnt.Payload.Status;    
                }
            });
        }

        private void AddInterviewStatus(InterviewSummary summary, InterviewStatus status, DateTime date,
            string comment, Guid responsibleId)
        {
            summary.CommentedStatusesHistory.Add(new InterviewCommentedStatus()
            {
                Status = status,
                Date = date,
                Comment = comment,
                Responsible = GetResponsibleIdName(responsibleId),
                ResponsibleId = responsibleId
            });
        }

        private string GetResponsibleIdName(Guid responsibleId)
        {
            var responsible = this.users.GetById(responsibleId);
            return responsible != null ? responsible.UserName : "<UNKNOWN RESPONSIBLE>";
        }
    }
}
