using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class StatusChangeHistoryDenormalizerFunctional :
        ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>,
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
        IUpdateHandler<InterviewSummary, AudioQuestionAnswered>,
        IUpdateHandler<InterviewSummary, InterviewResumed>,
        IUpdateHandler<InterviewSummary, InterviewPaused>,
        IUpdateHandler<InterviewSummary, InterviewOpenedBySupervisor>,
        IUpdateHandler<InterviewSummary, InterviewClosedBySupervisor>,
        IUpdateHandler<InterviewSummary, TranslationSwitched>
    {
        private readonly IUserViewFactory users;
        private readonly string unknown = "Unknown";
        private readonly InterviewExportedAction[] listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded = new[] { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor, InterviewExportedAction.Restarted };

        private InterviewSummary RecordFirstAnswerIfNeeded(Guid eventIdentifier, InterviewSummary interviewSummary, Guid interviewId, Guid userId, DateTime answerTime)
        {
            if(!interviewSummary.InterviewCommentedStatuses.Any())
                   return interviewSummary;

            if (!this.listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded.Contains(interviewSummary.InterviewCommentedStatuses.Last().Status))
                return interviewSummary;

            var responsible = this.users.GetUser(new UserViewInputModel(userId));

            if (responsible == null || !responsible.Roles.Contains(UserRoles.Interviewer))
                return interviewSummary;

            interviewSummary = this.AddCommentedStatus(
               eventIdentifier,
               interviewSummary,
               userId,
               interviewSummary.TeamLeadId,
               interviewSummary.ResponsibleId,
               InterviewExportedAction.FirstAnswerSet,
               answerTime,
               "");

            return interviewSummary;
        }

        public StatusChangeHistoryDenormalizerFunctional(
            IUserViewFactory users)
        {
            this.users = users;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewOnClientCreated> @event)
        {
            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Created,
                @event.EventTimeStamp,
                string.Empty);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewCreated> @event)
        {
            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Created,
                @event.Payload.CreationTime ?? @event.EventTimeStamp,
                string.Empty);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewFromPreloadedDataCreated> @event)
        {
            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                null,
                null,
                InterviewExportedAction.Created,
                @event.EventTimeStamp,
                string.Empty);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewRestarted> @event)
        {
            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                state.ResponsibleId,
                InterviewExportedAction.Restarted,
                @event.Payload.RestartTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SupervisorAssigned> @event)
        {
            var expectedStatus = InterviewExportedAction.SupervisorAssigned;

            var lastStatusInfo = state?.InterviewCommentedStatuses?.LastOrDefault();
            if (lastStatusInfo?.Status == InterviewExportedAction.Completed)
                expectedStatus = InterviewExportedAction.Completed;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                @event.Payload.SupervisorId,
                null,
                expectedStatus,
                @event.Payload.AssignTime ?? @event.EventTimeStamp,
                null);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewCompleted> @event)
        {
            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                state.ResponsibleId,
                InterviewExportedAction.Completed,
                @event.Payload.CompleteTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewRejected> @event)
        {
            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                state.ResponsibleId,
                InterviewExportedAction.RejectedBySupervisor,
                @event.Payload.RejectTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewApproved> @event)
        {
            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                state.ResponsibleId,
                InterviewExportedAction.ApprovedBySupervisor,
                @event.Payload.ApproveTime ?? @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewRejectedByHQ> @event)
        {
            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                state.ResponsibleId,
                InterviewExportedAction.RejectedByHeadquarter,
                @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewApprovedByHQ> @event)
        {
            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                state.ResponsibleId,
                InterviewExportedAction.ApprovedByHeadquarter,
                @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewerAssigned> @event)
        {
            if (@event.Payload.InterviewerId == null)
                return state;

            var expectedStatus = InterviewExportedAction.InterviewerAssigned;

            var lastStatusInfo = state?.InterviewCommentedStatuses?.LastOrDefault();
            if(lastStatusInfo?.Status == InterviewExportedAction.Completed)
                expectedStatus = InterviewExportedAction.Completed;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                @event.Payload.InterviewerId,
                expectedStatus,
                @event.Payload.AssignTime ?? @event.EventTimeStamp,
                null);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewDeleted> @event)
        {
            return null;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewHardDeleted> @event)
        {
            return null;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewRestored> @event)
        {
            if (@event.Origin == Constants.HeadquartersSynchronizationOrigin)
                return state;

            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                state.ResponsibleId,
                InterviewExportedAction.Restored,
                @event.EventTimeStamp,
                null);
        }

        private string GetResponsibleIdName(Guid responsibleId)
            => this.users.GetUser(new UserViewInputModel(responsibleId))?.UserName ?? this.unknown;

        private readonly List<InterviewExportedAction> actionsThatAreNotStatusChange = new List<InterviewExportedAction>
        {
            InterviewExportedAction.Paused,
            InterviewExportedAction.Resumed,
            InterviewExportedAction.OpenedBySupervisor,
            InterviewExportedAction.ClosedBySupervisor
        };

        private InterviewSummary AddCommentedStatus(
            Guid eventId,
            InterviewSummary state,
            Guid userId,
            Guid? supervisorId,
            Guid? interviewerId,
            InterviewExportedAction status,
            DateTime timestamp,
            string comment)
        {
            TimeSpan? timeSpanWithPreviousStatus = null;

            if (!actionsThatAreNotStatusChange.Contains(status) && state.InterviewCommentedStatuses.Any())
            {
                timeSpanWithPreviousStatus = timestamp - state.InterviewCommentedStatuses.Last(x => !actionsThatAreNotStatusChange.Contains(x.Status)).Timestamp;
            }
            var supervisorName = supervisorId.HasValue ? this.GetResponsibleIdName(supervisorId.Value) : "";
            var interviewerName = interviewerId.HasValue ? this.GetResponsibleIdName(interviewerId.Value) : "";

            var statusOriginator = this.users.GetUser(new UserViewInputModel(userId));

            state.InterviewCommentedStatuses.Add(new InterviewCommentedStatus(
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
                interviewerName,
                state.InterviewCommentedStatuses.Count,
                state));

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<TextQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericRealQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<DateTimeQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<TextListQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<PictureQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<UnapprovedByHeadquarters> @event)
        {
            return this.AddCommentedStatus(
                @event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                state.ResponsibleId,
                InterviewExportedAction.UnapprovedByHeadquarter,
                @event.EventTimeStamp,
                @event.Payload.Comment);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AreaQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<AudioQuestionAnswered> @event)
        {
            return this.RecordFirstAnswerIfNeeded(@event.EventIdentifier, state, @event.EventSourceId, @event.Payload.UserId, @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.AnswerTimeUtc.Value);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewResumed> @event)
        {
            return AddCommentedStatus(@event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                @event.Payload.UserId,
                InterviewExportedAction.Resumed,
                @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.UtcTime.Value,
                null);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewPaused> @event)
        {
            return AddCommentedStatus(@event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                @event.Payload.UserId,
                InterviewExportedAction.Paused,
                @event.Payload.OriginDate?.UtcDateTime ?? @event.Payload.UtcTime.Value,
                null);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<TranslationSwitched> @event)
        {
            return AddCommentedStatus(@event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                @event.Payload.UserId,
                InterviewExportedAction.TranslationSwitched,
                @event.EventTimeStamp,
                @event.Payload.Language);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewOpenedBySupervisor> @event)
        {
            return AddCommentedStatus(@event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                @event.Payload.UserId,
                InterviewExportedAction.OpenedBySupervisor,
                @event.EventTimeStamp,
                null);
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewClosedBySupervisor> @event)
        {
            return AddCommentedStatus(@event.EventIdentifier,
                state,
                @event.Payload.UserId,
                state.TeamLeadId,
                @event.Payload.UserId,
                InterviewExportedAction.ClosedBySupervisor,
                @event.EventTimeStamp,
                null);
        }
    }
}
