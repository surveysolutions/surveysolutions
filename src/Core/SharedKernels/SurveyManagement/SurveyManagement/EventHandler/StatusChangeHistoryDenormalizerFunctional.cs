using System;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
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
        IUpdateHandler<InterviewStatuses, InterviewCreated>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummares;


        public override object[] Readers
        {
            get { return new object[] {this.users, this.interviewSummares}; }
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
                InterviewStatus.Restarted,
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
                InterviewStatus.SupervisorAssigned,
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
                InterviewStatus.Completed,
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
                InterviewStatus.RejectedBySupervisor,
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
                InterviewStatus.ApprovedBySupervisor,
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
                InterviewStatus.RejectedByHeadquarters,
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
                InterviewStatus.ApprovedByHeadquarters,
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
                InterviewStatus.InterviewerAssigned,
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
                InterviewStatus.Deleted,
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
                InterviewStatus.Deleted,
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
                InterviewStatus.Restored,
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
            InterviewStatus status,
            DateTime timestamp,
            string comment,
            string responsibleName)
        {
            interviewStatuses.InterviewCommentedStatuses.Add(new InterviewCommentedStatus(
                userId,
                supervisorId,
                interviewerId,
                status,
                timestamp,
                comment,
                responsibleName));

            return interviewStatuses;
        }
    }
}
