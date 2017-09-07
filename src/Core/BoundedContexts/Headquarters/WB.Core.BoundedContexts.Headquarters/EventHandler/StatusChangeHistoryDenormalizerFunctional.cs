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
        AbstractFunctionalEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>,
        IUpdateHandler<InterviewSummary, InterviewerAssigned>,
        IUpdateHandler<InterviewSummary, InterviewCompleted>,
        IUpdateHandler<InterviewSummary, InterviewRejected>,
        IUpdateHandler<InterviewSummary, InterviewApproved>,
        IUpdateHandler<InterviewSummary, InterviewRejectedByHQ>,
        IUpdateHandler<InterviewSummary, InterviewApprovedByHQ>,
        IUpdateHandler<InterviewSummary, InterviewOnClientCreated>,
        IUpdateHandler<InterviewSummary, InterviewFromPreloadedDataCreated>,
        IUpdateHandler<InterviewSummary, InterviewRestarted>,
        IUpdateHandler<InterviewSummary, SupervisorAssigned>,
        IUpdateHandler<InterviewSummary, InterviewDeleted>,
        IUpdateHandler<InterviewSummary, InterviewHardDeleted>,
        IUpdateHandler<InterviewSummary, InterviewRestored>,
        IUpdateHandler<InterviewSummary, InterviewCreated>,
        IUpdateHandler<InterviewSummary, TextQuestionAnswered>,
        IUpdateHandler<InterviewSummary, MultipleOptionsQuestionAnswered>,
        IUpdateHandler<InterviewSummary, SingleOptionQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericRealQuestionAnswered>,
        IUpdateHandler<InterviewSummary, NumericIntegerQuestionAnswered>,
        IUpdateHandler<InterviewSummary, DateTimeQuestionAnswered>,
        IUpdateHandler<InterviewSummary, GeoLocationQuestionAnswered>,
        IUpdateHandler<InterviewSummary, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateHandler<InterviewSummary, SingleOptionLinkedQuestionAnswered>,
        IUpdateHandler<InterviewSummary, TextListQuestionAnswered>,
        IUpdateHandler<InterviewSummary, QRBarcodeQuestionAnswered>,
        IUpdateHandler<InterviewSummary, PictureQuestionAnswered>,
        IUpdateHandler<InterviewSummary, UnapprovedByHeadquarters>,
        IUpdateHandler<InterviewSummary, AreaQuestionAnswered>,
        IUpdateHandler<InterviewSummary, AudioQuestionAnswered>
    {
        private readonly IUserViewFactory users;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummares;
        private readonly string unknown = "Unknown";
        private readonly InterviewExportedAction[] listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded = new[] { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor, InterviewExportedAction.Restarted };

        public override object[] Readers => new object[] {this.interviewSummares};

        private InterviewSummary RecordFirstAnswerIfNeeded(Guid eventIdentifier, InterviewSummary InterviewSummary, Guid interviewId, Guid userId, DateTime answerTime)
        {
            if(!InterviewSummary.InterviewCommentedStatuses.Any())
                   return InterviewSummary;

            if (!this.listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded.Contains(InterviewSummary.InterviewCommentedStatuses.Last().Status))
                return InterviewSummary;

            var responsible = this.users.GetUser(new UserViewInputModel(userId));

            if (responsible == null || !responsible.Roles.Contains(UserRoles.Interviewer))
                return InterviewSummary;

            var interviewSummary = this.interviewSummares.GetById(interviewId);
            if (interviewSummary == null)
                return InterviewSummary;

            InterviewSummary = this.AddCommentedStatus(
               eventIdentifier,
               InterviewSummary,
               userId,
               interviewSummary.TeamLeadId,
               interviewSummary.ResponsibleId,
               InterviewExportedAction.FirstAnswerSet,
               answerTime,
               "");

            return InterviewSummary;
        }

        public StatusChangeHistoryDenormalizerFunctional(
            IReadSideRepositoryWriter<InterviewSummary> statuses,
            IUserViewFactory users,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummares)
            : base(statuses)
        {
            this.users = users;
            this.interviewSummares = interviewSummares;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewOnClientCreated> @event)
        {
            var InterviewSummary = this.CreateInterviewSummary(@event.EventSourceId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion);

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Created,
                @event.EventTimeStamp,
                string.Empty);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewCreated> @event)
        {
            var InterviewSummary = this.CreateInterviewSummary(@event.EventSourceId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion);

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Created,
                @event.EventTimeStamp,
                string.Empty);
        }

        public InterviewSummary Update(InterviewSummary state,
            IPublishedEvent<InterviewFromPreloadedDataCreated> @event)
        {
            var InterviewSummary = this.CreateInterviewSummary(@event.EventSourceId, @event.Payload.QuestionnaireId,
                @event.Payload.QuestionnaireVersion);

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Created,
                @event.EventTimeStamp,
                string.Empty);
        }

        public InterviewSummary Update(InterviewSummary InterviewSummary, IPublishedEvent<InterviewRestarted> @event)
        {
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return InterviewSummary;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.Restarted,
                 @event.Payload.RestartTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary InterviewSummary, IPublishedEvent<SupervisorAssigned> @event)
        {
            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                @event.Payload.SupervisorId,
                null,
                InterviewExportedAction.SupervisorAssigned,
                @event.EventTimeStamp,
                null);
        }

        public InterviewSummary Update(InterviewSummary InterviewSummary, IPublishedEvent<InterviewCompleted> @event)
        {
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return InterviewSummary;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.Completed,
                @event.Payload.CompleteTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary InterviewSummary, IPublishedEvent<InterviewRejected> @event)
        {
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return InterviewSummary;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.RejectedBySupervisor,
                @event.Payload.RejectTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary InterviewSummary, IPublishedEvent<InterviewApproved> @event)
        {
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return InterviewSummary;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.ApprovedBySupervisor,
                @event.Payload.ApproveTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary InterviewSummary, IPublishedEvent<InterviewRejectedByHQ> @event)
        {
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return InterviewSummary;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.RejectedByHeadquarter,
                @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary InterviewSummary, IPublishedEvent<InterviewApprovedByHQ> @event)
        {
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return InterviewSummary;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.ApprovedByHeadquarter,
                @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary InterviewSummary, IPublishedEvent<InterviewerAssigned> @event)
        {
            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return InterviewSummary;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                @event.Payload.InterviewerId,
                InterviewExportedAction.InterviewerAssigned,
                @event.Payload.AssignTime ?? @event.EventTimeStamp,
                null);
        }

        public InterviewSummary Update(InterviewSummary InterviewSummary, IPublishedEvent<InterviewDeleted> @event)
        {
            return null;
        }

        public InterviewSummary Update(InterviewSummary InterviewSummary, IPublishedEvent<InterviewHardDeleted> @event)
        {
            return null;
        }

        public InterviewSummary Update(InterviewSummary InterviewSummary, IPublishedEvent<InterviewRestored> @event)
        {
            if (@event.Origin == Constants.HeadquartersSynchronizationOrigin)
                return InterviewSummary;

            var interviewSummary = this.interviewSummares.GetById(@event.EventSourceId);
            if (interviewSummary == null)
                return InterviewSummary;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                InterviewSummary,
                @event.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewExportedAction.Restored,
                @event.EventTimeStamp,
                null);
        }

        private string GetResponsibleIdName(Guid responsibleId)
            => this.users.GetUser(new UserViewInputModel(responsibleId))?.UserName ?? this.unknown;

        private InterviewSummary CreateInterviewSummary(Guid interviewId, Guid questionnaireId, long questionnaireVersion)
        {
            return
                new InterviewSummary()
                {
                    InterviewId = interviewId,
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion
                };
        }

        private InterviewSummary AddCommentedStatus(
            Guid eventId,
            InterviewSummary InterviewSummary,
            Guid userId,
            Guid? supervisorId,
            Guid? interviewerId,
            InterviewExportedAction status,
            DateTime timestamp,
            string comment)
        {
            TimeSpan? timeSpanWithPreviousStatus = null;

            if (InterviewSummary.InterviewCommentedStatuses.Any())
            {
                timeSpanWithPreviousStatus = timestamp - InterviewSummary.InterviewCommentedStatuses.Last().Timestamp;
            }
            var supervisorName = supervisorId.HasValue ? this.GetResponsibleIdName(supervisorId.Value) : "";
            var interviewerName = interviewerId.HasValue ? this.GetResponsibleIdName(interviewerId.Value) : "";

            var statusOriginator = this.users.GetUser(new UserViewInputModel(userId));

            InterviewSummary.InterviewCommentedStatuses.Add(new InterviewCommentedStatus(
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

            return InterviewSummary;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<TextQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<TextListQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<PictureQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<UnapprovedByHeadquarters> @event)
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

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AreaQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AudioQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.AnswerTimeUtc);
        }
    }
}
