using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
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
        IUpdateHandler<InterviewStatuses, PictureQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, UnapprovedByHeadquarters>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummares;
        private readonly string unknown = "Unknown";
        private readonly InterviewExportedAction[] listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded = new[] { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor, InterviewExportedAction.Restarted };

        public override object[] Readers
        {
            get { return new object[] {this.users, this.interviewSummares}; }
        }

        private InterviewStatuses RecordFirstAnswerIfNeeded(Guid eventIdentifier, InterviewStatuses interviewStatuses, Guid interviewId, Guid userId, DateTime answerTime)
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
               eventIdentifier,
               interviewStatuses,
               userId,
               interviewSummary.TeamLeadId,
               interviewSummary.ResponsibleId,
               InterviewExportedAction.FirstAnswerSet,
               answerTime,
               "");

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

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<InterviewOnClientCreated> @event)
        {
            return CreateInterviewStatuses(@event.EventSourceId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<InterviewCreated> @event)
        {
            return CreateInterviewStatuses(@event.EventSourceId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion);
        }

        public InterviewStatuses Update(InterviewStatuses state,
            IPublishedEvent<InterviewFromPreloadedDataCreated> @event)
        {
            return CreateInterviewStatuses(@event.EventSourceId, @event.Payload.QuestionnaireId,
               @event.Payload.QuestionnaireVersion);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewRestarted> @event)
        {
            var interviewSummary = interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.Restarted,
                 @event.Payload.RestartTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<SupervisorAssigned> @event)
        {
            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                @event.Payload.SupervisorId,
                null,
                InterviewExportedAction.SupervisorAssigned,
                @event.EventTimeStamp,
                null);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewCompleted> @event)
        {
            var interviewSummary = interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.Completed,
                @event.Payload.CompleteTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewRejected> @event)
        {
            var interviewSummary = interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.RejectedBySupervisor,
                @event.Payload.RejectTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewApproved> @event)
        {
            var interviewSummary = interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.ApprovedBySupervisor,
                @event.Payload.ApproveTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewRejectedByHQ> @event)
        {
            var interviewSummary = interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.RejectedByHeadquarter,
                @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewApprovedByHQ> @event)
        {
            var interviewSummary = interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.ApprovedByHeadquarter,
                @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewerAssigned> @event)
        {
            var interviewSummary = interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                @event.Payload.InterviewerId,
                InterviewExportedAction.InterviewerAssigned,
                @event.Payload.AssignTime ?? @event.EventTimeStamp,
                null);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewDeleted> @event)
        {
            if (@event.Origin == Constants.HeadquartersSynchronizationOrigin)
                return interviewStatuses;

            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Deleted,
                @event.EventTimeStamp,
                null);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewHardDeleted> @event)
        {
            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Deleted,
                @event.EventTimeStamp,
                null);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewRestored> @event)
        {
            if (@event.Origin == Constants.HeadquartersSynchronizationOrigin)
                return interviewStatuses;

            var interviewSummary = interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.Restored,
                @event.EventTimeStamp,
                null);
        }

        private string GetResponsibleIdName(Guid responsibleId)
        {
            var userDocument = this.users.GetById(responsibleId);
            var userName = userDocument?.UserName;
            return userName ?? this.unknown;
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
            Guid eventId,
            InterviewStatuses interviewStatuses,
            Guid userId,
            Guid? supervisorId,
            Guid? interviewerId,
            InterviewExportedAction status,
            DateTime timestamp,
            string comment)
        {
            TimeSpan? timeSpanWithPreviousStatus = null;

            if (interviewStatuses.InterviewCommentedStatuses.Any())
            {
                timeSpanWithPreviousStatus = timestamp - interviewStatuses.InterviewCommentedStatuses.Last().Timestamp;
            }
            var supervisorName = supervisorId.HasValue ? GetResponsibleIdName(supervisorId.Value) : "";
            var interviewerName = interviewerId.HasValue ? GetResponsibleIdName(interviewerId.Value) : "";

            var statusOriginator = this.users.GetById(userId);

            interviewStatuses.InterviewCommentedStatuses.Add(new InterviewCommentedStatus(
                eventId,
                userId,
                supervisorId,
                interviewerId,
                status,
                timestamp,
                comment,
                statusOriginator == null ? unknown : statusOriginator.UserName,
                statusOriginator == null || !statusOriginator.Roles.Any()
                    ? UserRoles.Undefined
                    : statusOriginator.Roles.First(),
                timeSpanWithPreviousStatus,
                supervisorName,
                interviewerName));

            return interviewStatuses;
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<TextQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<TextListQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<PictureQuestionAnswered> @event)
        {
            return RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<UnapprovedByHeadquarters> @event)
        {
            var interviewSummary = interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.UnapprovedByHeadquarter,
                @event.EventTimeStamp,
                @event.Payload.Comment);
        }
    }
}
