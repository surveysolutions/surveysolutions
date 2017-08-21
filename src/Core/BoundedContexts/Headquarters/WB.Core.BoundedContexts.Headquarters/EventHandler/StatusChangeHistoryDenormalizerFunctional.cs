using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
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
        IUpdateHandler<InterviewStatuses, UnapprovedByHeadquarters>,
        IUpdateHandler<InterviewStatuses, AreaQuestionAnswered>,
        IUpdateHandler<InterviewStatuses, AudioQuestionAnswered>
    {
        private readonly IUserViewFactory users;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummares;
        private readonly string unknown = "Unknown";
        private readonly InterviewExportedAction[] listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded = new[] { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor, InterviewExportedAction.Restarted };

        public override object[] Readers => new object[] {this.interviewSummares};

        private InterviewStatuses RecordFirstAnswerIfNeeded(Guid eventIdentifier, InterviewStatuses interviewStatuses, Guid interviewId, Guid userId, DateTime answerTime)
        {
            if(!interviewStatuses.InterviewCommentedStatuses.Any())
                   return interviewStatuses;

            if (!this.listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded.Contains(interviewStatuses.InterviewCommentedStatuses.Last().Status))
                return interviewStatuses;

            var responsible = this.users.GetUser(new UserViewInputModel(userId));

            if (responsible == null || !responsible.Roles.Contains(UserRoles.Interviewer))
                return interviewStatuses;

            var interviewSummary = this.interviewSummares.GetById(interviewId);
            if (interviewSummary == null)
                return interviewStatuses;

            interviewStatuses = this.AddCommentedStatus(
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
            IUserViewFactory users,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummares)
            : base(statuses)
        {
            this.users = users;
            this.interviewSummares = interviewSummares;
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<InterviewOnClientCreated> @event)
        {
            var interviewStatuses = this.CreateInterviewStatuses(@event.EventSourceId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion);

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Created,
                @event.EventTimeStamp,
                string.Empty);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<InterviewCreated> @event)
        {
            var interviewStatuses = this.CreateInterviewStatuses(@event.EventSourceId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion);

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Created,
                @event.EventTimeStamp,
                string.Empty);
        }

        public InterviewStatuses Update(InterviewStatuses state,
            IPublishedEvent<InterviewFromPreloadedDataCreated> @event)
        {
            var interviewStatuses = this.CreateInterviewStatuses(@event.EventSourceId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion);

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                interviewStatuses,
                @event.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Created,
                @event.EventTimeStamp,
                string.Empty);
        }

        public InterviewStatuses Update(InterviewStatuses interviewStatuses, IPublishedEvent<InterviewRestarted> @event)
        {
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return this.AddCommentedStatus(
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
            return this.AddCommentedStatus(
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
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return this.AddCommentedStatus(
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
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return this.AddCommentedStatus(
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
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return this.AddCommentedStatus(
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
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return this.AddCommentedStatus(
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
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return this.AddCommentedStatus(
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
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return this.AddCommentedStatus(
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

            return this.AddCommentedStatus(
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
            return this.AddCommentedStatus(
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

            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return interviewStatuses;

            return this.AddCommentedStatus(
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
            => this.users.GetUser(new UserViewInputModel(responsibleId))?.UserName ?? this.unknown;

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
            var supervisorName = supervisorId.HasValue ? this.GetResponsibleIdName(supervisorId.Value) : "";
            var interviewerName = interviewerId.HasValue ? this.GetResponsibleIdName(interviewerId.Value) : "";

            var statusOriginator = this.users.GetUser(new UserViewInputModel(userId));

            interviewStatuses.InterviewCommentedStatuses.Add(new InterviewCommentedStatus(
                eventId,
                userId,
                supervisorId,
                interviewerId,
                status,
                timestamp,
                comment,
                statusOriginator == null ? this.unknown : statusOriginator.UserName,
                statusOriginator == null || !statusOriginator.Roles.Any()
                    ? 0
                    : statusOriginator.Roles.First(),
                timeSpanWithPreviousStatus,
                supervisorName,
                interviewerName));

            return interviewStatuses;
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<TextQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<TextListQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<PictureQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<UnapprovedByHeadquarters> @event)
        {
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return state;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.UnapprovedByHeadquarter,
                @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<AreaQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewStatuses Update(InterviewStatuses state, IPublishedEvent<AudioQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }
    }
}
