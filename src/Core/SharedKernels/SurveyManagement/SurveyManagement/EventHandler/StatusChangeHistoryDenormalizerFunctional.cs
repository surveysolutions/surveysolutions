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
        BaseDenormalizer,
        IEventHandler<InterviewerAssigned>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewRejected>,
        IEventHandler<InterviewApproved>,
        IEventHandler<InterviewRejectedByHQ>,
        IEventHandler<InterviewApprovedByHQ>,
        IEventHandler<InterviewOnClientCreated>,
        IEventHandler<InterviewFromPreloadedDataCreated>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<SupervisorAssigned>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewHardDeleted>,
        IEventHandler<InterviewRestored>,
        IEventHandler<InterviewCreated>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummares;
        private readonly IReadSideRepositoryWriter<InterviewStatuses> statuses;


        public override object[] Readers
        {
            get { return new object[] { this.users, this.interviewSummares }; }
        }

        public override object[] Writers
        {
            get { return new[] {this.statuses}; }
        }

        public StatusChangeHistoryDenormalizerFunctional(IReadSideRepositoryWriter<InterviewStatuses> statuses,
            IReadSideRepositoryWriter<UserDocument> users, IReadSideRepositoryWriter<InterviewSummary> interviewSummares)
        {
            this.statuses = statuses;
            this.users = users;
            this.interviewSummares = interviewSummares;
        }

        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            this.statuses.Store(
                new InterviewStatuses()
                {
                    InterviewId = evnt.EventSourceId.FormatGuid(),
                    QuestionnaireId = evnt.Payload.QuestionnaireId,
                    QuestionnaireVersion = evnt.Payload.QuestionnaireVersion
                }, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            this.statuses.Store(
                new InterviewStatuses()
                {
                    InterviewId = evnt.EventSourceId.FormatGuid(),
                    QuestionnaireId = evnt.Payload.QuestionnaireId,
                    QuestionnaireVersion = evnt.Payload.QuestionnaireVersion
                }, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            this.statuses.Store(
               new InterviewStatuses()
               {
                   InterviewId = evnt.EventSourceId.FormatGuid(),
                   QuestionnaireId = evnt.Payload.QuestionnaireId,
                   QuestionnaireVersion = evnt.Payload.QuestionnaireVersion
               }, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            var interviewStatuses = statuses.GetById(evnt.EventSourceId);
            if(interviewStatuses==null)
                return;

            interviewStatuses.InterviewCommentedStatuses.Add(new InterviewCommentedStatus(
                evnt.Payload.UserId, 
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewStatus.Restarted,
                evnt.Payload.RestartTime ?? evnt.EventTimeStamp, 
                evnt.Payload.Comment,
                GetResponsibleIdName(evnt.Payload.UserId)));

            this.statuses.Store(interviewStatuses, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            var interviewStatuses = statuses.GetById(evnt.EventSourceId);
            if (interviewStatuses == null)
                return;

            interviewStatuses.InterviewCommentedStatuses.Add(new InterviewCommentedStatus(
                evnt.Payload.UserId,
                evnt.Payload.SupervisorId,
                null,
                InterviewStatus.SupervisorAssigned,
                evnt.EventTimeStamp, 
                null, 
                GetResponsibleIdName(evnt.Payload.UserId)));

            this.statuses.Store(interviewStatuses, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            var interviewStatuses = statuses.GetById(evnt.EventSourceId);
            if (interviewStatuses == null)
                return;

            interviewStatuses.InterviewCommentedStatuses.Add(
                new InterviewCommentedStatus(
                    evnt.Payload.UserId, 
                    interviewSummary.TeamLeadId,
                    interviewSummary.ResponsibleId,
                    InterviewStatus.Completed,
                    evnt.Payload.CompleteTime ?? evnt.EventTimeStamp, 
                    evnt.Payload.Comment,
                    GetResponsibleIdName(evnt.Payload.UserId)));

            this.statuses.Store(interviewStatuses, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            var interviewStatuses = statuses.GetById(evnt.EventSourceId);
            if (interviewStatuses == null)
                return;

            interviewStatuses.InterviewCommentedStatuses.Add(new InterviewCommentedStatus(evnt.Payload.UserId,
                interviewSummary.TeamLeadId,
                interviewSummary.ResponsibleId,
                InterviewStatus.RejectedBySupervisor,
                evnt.EventTimeStamp, 
                evnt.Payload.Comment,
                GetResponsibleIdName(evnt.Payload.UserId)));

            this.statuses.Store(interviewStatuses, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewApproved> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            var interviewStatuses = statuses.GetById(evnt.EventSourceId);
            if (interviewStatuses == null)
                return;

            interviewStatuses.InterviewCommentedStatuses.Add(
                new InterviewCommentedStatus(
                    evnt.Payload.UserId, 
                    interviewSummary.TeamLeadId,
                    interviewSummary.ResponsibleId,
                    InterviewStatus.ApprovedBySupervisor,
                    evnt.EventTimeStamp, 
                    evnt.Payload.Comment,
                    GetResponsibleIdName(evnt.Payload.UserId)));

            this.statuses.Store(interviewStatuses, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            var interviewStatuses = statuses.GetById(evnt.EventSourceId);
            if (interviewStatuses == null)
                return;

            interviewStatuses.InterviewCommentedStatuses.Add(
                new InterviewCommentedStatus(
                    evnt.Payload.UserId, 
                    interviewSummary.TeamLeadId,
                    interviewSummary.ResponsibleId,
                    InterviewStatus.RejectedByHeadquarters,
                    evnt.EventTimeStamp, 
                    evnt.Payload.Comment,
                    GetResponsibleIdName(evnt.Payload.UserId)));

            this.statuses.Store(interviewStatuses, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            var interviewStatuses = statuses.GetById(evnt.EventSourceId);
            if (interviewStatuses == null)
                return;

            interviewStatuses.InterviewCommentedStatuses.Add(
                new InterviewCommentedStatus(
                    evnt.Payload.UserId, 
                    interviewSummary.TeamLeadId,
                    interviewSummary.ResponsibleId,
                    InterviewStatus.ApprovedByHeadquarters,
                    evnt.EventTimeStamp, 
                    evnt.Payload.Comment,
                    GetResponsibleIdName(evnt.Payload.UserId)));

            this.statuses.Store(interviewStatuses, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            var interviewStatuses = statuses.GetById(evnt.EventSourceId);
            if (interviewStatuses == null)
                return;

            interviewStatuses.InterviewCommentedStatuses.Add(
                new InterviewCommentedStatus(
                    evnt.Payload.UserId, 
                    interviewSummary.TeamLeadId,
                    evnt.Payload.InterviewerId,
                    InterviewStatus.InterviewerAssigned,
                    evnt.EventTimeStamp, 
                    null,
                    GetResponsibleIdName(evnt.Payload.UserId)));

            this.statuses.Store(interviewStatuses, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            if (evnt.Origin == Constants.HeadquartersSynchronizationOrigin)
                return;
          
            var interviewStatuses = statuses.GetById(evnt.EventSourceId);
            if (interviewStatuses == null)
                return;

            interviewStatuses.InterviewCommentedStatuses.Add(
                new InterviewCommentedStatus(
                    evnt.Payload.UserId, 
                    null,
                    null,
                    InterviewStatus.Deleted,
                    evnt.EventTimeStamp, 
                    null,
                    null));

            this.statuses.Store(interviewStatuses, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            var interviewStatuses = statuses.GetById(evnt.EventSourceId);
            if (interviewStatuses == null)
                return;

            interviewStatuses.InterviewCommentedStatuses.Add(
                new InterviewCommentedStatus(
                    evnt.Payload.UserId, 
                    null,
                    null,
                    InterviewStatus.Deleted,
                    evnt.EventTimeStamp, 
                    null, 
                    null));

            this.statuses.Store(interviewStatuses, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewRestored> evnt)
        {
            if (evnt.Origin == Constants.HeadquartersSynchronizationOrigin)
                return;

            var interviewSummary = interviewSummares.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
                return;

            var interviewStatuses = statuses.GetById(evnt.EventSourceId);
            if (interviewStatuses == null)
                return;

            interviewStatuses.InterviewCommentedStatuses.Add(
                new InterviewCommentedStatus(
                    evnt.Payload.UserId, 
                    interviewSummary.TeamLeadId,
                    interviewSummary.ResponsibleId,
                    InterviewStatus.Restored,
                    evnt.EventTimeStamp, 
                    null,
                    GetResponsibleIdName(evnt.Payload.UserId)));

            this.statuses.Store(interviewStatuses, evnt.EventSourceId);
        }

        private string GetResponsibleIdName(Guid responsibleId)
        {
            return Monads.Maybe(() => this.users.GetById(responsibleId).UserName) ?? "Unknown";
        }
    }
}
