using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class StatusChangeHistoryDenormalizerFunctional :
        AbstractFunctionalEventHandler<InterviewStatuses, IReadSideRepositoryWriter<InterviewStatuses>>,
        IUpdateHandler<InterviewStatuses, InterviewerAssigned>,
        IUpdateHandler<InterviewStatuses, InterviewCompleted>,
        IUpdateHandler<InterviewStatuses, InterviewRejected>,
        IUpdateHandler<InterviewStatuses, InterviewApproved>,
        IUpdateHandler<InterviewStatuses, InterviewRejectedByHQ>,
        IUpdateHandler<InterviewStatuses, InterviewApprovedByHQ>,
        IUpdateHandler<InterviewStatuses, InterviewOnClientCreated>,
        IUpdateHandler<InterviewStatuses, InterviewFromPreloadedDataCreated>,
        IUpdateHandler<InterviewStatuses, InterviewRestarted>,
        IUpdateHandler<InterviewStatuses, SupervisorAssigned>,
        IUpdateHandler<InterviewStatuses, InterviewDeleted>,
        IUpdateHandler<InterviewStatuses, InterviewHardDeleted>,
        IUpdateHandler<InterviewStatuses, InterviewRestored>,
        IUpdateHandler<InterviewStatuses, InterviewCreated>,
        IUpdateHandler<InterviewStatuses, TextQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, DateTimeQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, GeoLocationQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, SingleOptionLinkedQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, TextListQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, QRBarcodeQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, PictureQuestionAnswered>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummares;

        private readonly InterviewExportedAction[] listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded = new[] { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor, InterviewExportedAction.Restarted };

        public override object[] Readers
        {
            get { return new object[] {this.users, this.interviewSummares}; }
        }

        private InterviewStatuses RecordFirstAnswerIfNeeded(InterviewStatuses interviewStatuses, Guid interviewId, Guid userId, DateTime answerTime)
        {
            if(!interviewStatuses.InterviewCommentedStatuses.Any())
                   return interviewStatuses;

            if (!listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded.Contains(interviewStatuses.InterviewCommentedStatuses.Last().Status))
                return interviewStatuses;

            UserDocument responsible = this.users.GetById(userId);

            if (responsible == null || !responsible.Roles.Contains(UserRoles.Operator))
                return interviewStatuses;

            var interviewSummary = interviewSummares.GetById(interviewId);
            if (interviewSummary == null)
                return interviewStatuses;

            interviewStatuses = AddCommentedStatus(
               interviewStatuses,
               userId,
               interviewSummary.TeamLeadId,
               interviewSummary.ResponsibleId,
               InterviewExportedAction.FirstAnswerSet,
               answerTime,
               "",
               GetResponsibleIdName(userId));

            return interviewStatuses;
        }

        public StatusChangeHistoryDenormalizerFunctional(
            IReadSideRepositoryWriter<InterviewStatuses> statuses,
            IReadSideRepositoryWriter<UserDocument> users,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummares)
            : base(statuses)
        {
            this.users = users;
            this.interviewSummares = interviewSummares;
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            return CreateInterviewStatuses(evnt.EventSourceId, evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<InterviewCreated> evnt)
        {
            return CreateInterviewStatuses(evnt.EventSourceId, evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion);
        }

        public InterviewStatuses Update(InterviewStatuses currentState,
            IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            return CreateInterviewStatuses(evnt.EventSourceId, evnt.Payload.QuestionnaireId,
               evnt.Payload.QuestionnaireVersion);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewRestarted> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                interviewStatuses,
                evnt.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.Restarted,
                 evnt.Payload.RestartTime ?? evnt.EventTimeStamp,
                evnt.Payload.Comment,
                GetResponsibleIdName(evnt.Payload.UserId));
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<SupervisorAssigned> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                interviewStatuses,
                evnt.Payload.UserId,
                evnt.Payload.SupervisorId,
                null,
                InterviewExportedAction.SupervisorAssigned,
                evnt.EventTimeStamp,
                null,
                GetResponsibleIdName(evnt.Payload.UserId));
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewCompleted> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                interviewStatuses,
                evnt.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.Completed,
                evnt.Payload.CompleteTime ?? evnt.EventTimeStamp,
                evnt.Payload.Comment,
                GetResponsibleIdName(evnt.Payload.UserId));
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewRejected> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                interviewStatuses,
                evnt.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.RejectedBySupervisor,
                evnt.Payload.RejectTime ?? evnt.EventTimeStamp,
                evnt.Payload.Comment,
                GetResponsibleIdName(evnt.Payload.UserId));
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewApproved> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                interviewStatuses,
                evnt.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.ApprovedBySupervisor,
                evnt.Payload.ApproveTime ?? evnt.EventTimeStamp,
                evnt.Payload.Comment,
                GetResponsibleIdName(evnt.Payload.UserId));
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                interviewStatuses,
                evnt.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.RejectedByHeadquarter,
                evnt.EventTimeStamp,
                evnt.Payload.Comment,
                GetResponsibleIdName(evnt.Payload.UserId));
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                interviewStatuses,
                evnt.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.ApprovedByHeadquarter,
                evnt.EventTimeStamp,
                evnt.Payload.Comment,
                GetResponsibleIdName(evnt.Payload.UserId));
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewerAssigned> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                interviewStatuses,
                evnt.Payload.UserId,
                interviewSummary.TeamLeadId,
                evnt.Payload.InterviewerId,
                InterviewExportedAction.InterviewerAssigned,
                evnt.Payload.AssignTime ?? evnt.EventTimeStamp,
                null,
                GetResponsibleIdName(evnt.Payload.UserId));
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewDeleted> evnt)
        {
            if (evnt.Origin == Constants.HeadquartersSynchronizationOrigin)
                return interviewStatuses;

            return AddCommentedStatus(
                interviewStatuses,
                evnt.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Deleted,
                evnt.EventTimeStamp,
                null,
                null);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewHardDeleted> evnt)
        {
            return AddCommentedStatus(
                interviewStatuses,
                evnt.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Deleted,
                evnt.EventTimeStamp,
                null,
                null);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewRestored> evnt)
        {
            if (evnt.Origin == Constants.HeadquartersSynchronizationOrigin)
                return interviewStatuses;

            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                interviewStatuses,
                evnt.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.Restored,
                evnt.EventTimeStamp,
                null,
                GetResponsibleIdName(evnt.Payload.UserId));
        }

        private string GetResponsibleIdName(Guid responsibleId)
        {
            return Monads.Maybe(() => this.users.GetById(responsibleId).UserName) ?? "Unknown";
        }

        private InterviewStatuses CreateInterviewStatuses(Guid interviewId, Guid questionnaireId, long questionnaireVersion)
        {
            return
                new InterviewStatuses()
                {
                    InterviewId = interviewId.FormatGuid(),
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion
                };
        }

        private InterviewStatuses AddCommentedStatus(
            InterviewStatuses interviewStatuses,
            Guid userId,
            Guid? supervisorId,
            Guid? interviewerId,
            InterviewExportedAction status,
            DateTime timestamp,
            string comment,
            string responsibleName)
        {
            TimeSpan? timeSpanWithPreviousStatus = null;

            if (interviewStatuses.InterviewCommentedStatuses.Any())
            {
                timeSpanWithPreviousStatus = timestamp - interviewStatuses.InterviewCommentedStatuses.Last().Timestamp;
            }
            var supervisorName = supervisorId.HasValue ? GetResponsibleIdName(supervisorId.Value) : "";
            var interviewerName = interviewerId.HasValue ? GetResponsibleIdName(interviewerId.Value) : "";
            interviewStatuses.InterviewCommentedStatuses.Add(new InterviewCommentedStatus(
                userId,
                supervisorId,
                interviewerId,
                status,
                timestamp,
                comment,
                responsibleName,
                timeSpanWithPreviousStatus,
                supervisorName,
                interviewerName));

            return interviewStatuses;
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<TextQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public InterviewStatuses Update(InterviewStatuses currentState, IPublishedEvent<PictureQuestionAnswered> evnt)
        {
            return RecordFirstAnswerIfNeeded(currentState, evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }
    }
}
