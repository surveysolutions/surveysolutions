using System;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class InterviewStatusChangeHistoryDenormalizer : AbstractFunctionalEventHandler<InterviewStatusHistory, IReadSideKeyValueStorage<InterviewStatusHistory>>,
        IUpdateHandler<InterviewStatusHistory, InterviewStatusChanged>,
        IUpdateHandler<InterviewStatusHistory, InterviewerAssigned>,
        IUpdateHandler<InterviewStatusHistory, InterviewCompleted>,
        IUpdateHandler<InterviewStatusHistory, InterviewRejected>,
        IUpdateHandler<InterviewStatusHistory, InterviewApproved>,
        IUpdateHandler<InterviewStatusHistory, InterviewRejectedByHQ>,
        IUpdateHandler<InterviewStatusHistory, InterviewApprovedByHQ>,
        IUpdateHandler<InterviewStatusHistory, InterviewOnClientCreated>,
        IUpdateHandler<InterviewStatusHistory, InterviewFromPreloadedDataCreated>,
        IUpdateHandler<InterviewStatusHistory, InterviewRestarted>,
        IUpdateHandler<InterviewStatusHistory, SupervisorAssigned>,
        IUpdateHandler<InterviewStatusHistory, InterviewDeleted>,
        IUpdateHandler<InterviewStatusHistory, InterviewHardDeleted>,
        IUpdateHandler<InterviewStatusHistory, InterviewRestored>,
        IUpdateHandler<InterviewStatusHistory, InterviewCreated>
    {
        private readonly IReadSideRepositoryReader<UserDocument> users;

        public InterviewStatusChangeHistoryDenormalizer(IReadSideKeyValueStorage<InterviewStatusHistory> readSideStorage,
            IReadSideRepositoryReader<UserDocument> users)
            : base(readSideStorage)
        {
            this.users = users;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var lastStatusChange = currentState.StatusChangeHistory.LastOrDefault();
            if (lastStatusChange != null && lastStatusChange.Status == evnt.Payload.Status)
            {
                lastStatusChange.Comment = evnt.Payload.Comment;
            }
            return currentState;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            var result = CreateStatusHistory(evnt.EventSourceId);
            AddInterviewStatus(result, InterviewStatus.Created, evnt.EventTimeStamp, null, evnt.Payload.UserId);
            return result;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewCreated> evnt)
        {
            var result = CreateStatusHistory(evnt.EventSourceId);
            AddInterviewStatus(result, InterviewStatus.Created, evnt.EventTimeStamp, null, evnt.Payload.UserId);
            return result;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            var result = CreateStatusHistory(evnt.EventSourceId);
            AddInterviewStatus(result, InterviewStatus.Created, evnt.EventTimeStamp, null, evnt.Payload.UserId);
            return result;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewRestarted> evnt)
        {
            AddInterviewStatus(currentState, InterviewStatus.Restarted, evnt.Payload.RestartTime ?? evnt.EventTimeStamp, null, evnt.Payload.UserId);
            return currentState;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<SupervisorAssigned> evnt)
        {
            AddInterviewStatus(currentState, InterviewStatus.SupervisorAssigned, evnt.EventTimeStamp, null, evnt.Payload.UserId);
            return currentState;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewCompleted> evnt)
        {
            AddInterviewStatus(currentState, InterviewStatus.Completed, evnt.Payload.CompleteTime ?? evnt.EventTimeStamp, null, evnt.Payload.UserId);
            return currentState;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewRejected> evnt)
        {
            AddInterviewStatus(currentState, InterviewStatus.RejectedBySupervisor, evnt.EventTimeStamp, evnt.Payload.Comment, evnt.Payload.UserId);
            return currentState;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewApproved> evnt)
        {
            AddInterviewStatus(currentState, InterviewStatus.ApprovedBySupervisor, evnt.EventTimeStamp, evnt.Payload.Comment, evnt.Payload.UserId);
            return currentState;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            AddInterviewStatus(currentState, InterviewStatus.RejectedByHeadquarters, evnt.EventTimeStamp, evnt.Payload.Comment, evnt.Payload.UserId);
            return currentState;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            AddInterviewStatus(currentState, InterviewStatus.ApprovedByHeadquarters, evnt.EventTimeStamp, evnt.Payload.Comment, evnt.Payload.UserId);
            return currentState;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewerAssigned> evnt)
        {
            AddInterviewStatus(currentState, InterviewStatus.InterviewerAssigned, evnt.EventTimeStamp, null, evnt.Payload.UserId);
            return currentState;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewDeleted> evnt)
        {
            if (evnt.Origin != Constants.HeadquartersSynchronizationOrigin)
            {
                AddInterviewStatus(currentState, InterviewStatus.Deleted, evnt.EventTimeStamp, null, evnt.Payload.UserId);
            }

            return currentState;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewHardDeleted> evnt)
        {
            AddInterviewStatus(currentState, InterviewStatus.Deleted, evnt.EventTimeStamp, null, evnt.Payload.UserId);
            return currentState;
        }

        public InterviewStatusHistory Update(InterviewStatusHistory currentState, IPublishedEvent<InterviewRestored> evnt)
        {
            if (evnt.Origin != Constants.HeadquartersSynchronizationOrigin)
            {
                AddInterviewStatus(currentState, InterviewStatus.Restored, evnt.EventTimeStamp, null, evnt.Payload.UserId);
            }
            return currentState;
        }

        private void AddInterviewStatus(InterviewStatusHistory interviewHistory, InterviewStatus status, DateTime date, string comment, Guid responsibleId)
        {
            interviewHistory.StatusChangeHistory.Add(new InterviewCommentedStatus
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
            return Monads.Maybe(() => this.users.GetById(responsibleId).UserName) ?? "Unknown";
        }

        private InterviewStatusHistory CreateStatusHistory(Guid interviewId)
        {
            return new InterviewStatusHistory(interviewId.FormatGuid());
        }

        public override object[] Readers
        {
            get { return new []{ this.users }; }
        }
    }
}
