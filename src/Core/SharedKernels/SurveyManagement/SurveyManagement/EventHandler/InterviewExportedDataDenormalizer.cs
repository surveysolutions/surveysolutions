using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class InterviewExportedDataDenormalizer : 
        BaseDenormalizer, IAtomicEventHandler,
        IEventHandler<InterviewApprovedByHQ>, 
        IEventHandler<SupervisorAssigned>,
        IEventHandler<InterviewerAssigned>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<InterviewApproved>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewHardDeleted>,
        IEventHandler<InterviewRejected>,
        IEventHandler<InterviewRejectedByHQ>,
        IEventHandler<InterviewRestored>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly IReadSideRepositoryWriter<InterviewStatuses> statuses;
        private readonly InterviewExportedAction[] listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded = new[] { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor, InterviewExportedAction.Restarted };

        private readonly IDataExportRepositoryWriter dataExportWriter;

        public InterviewExportedDataDenormalizer(IDataExportRepositoryWriter dataExportWriter,
            IReadSideRepositoryWriter<UserDocument> userDocumentWriter, 
            IReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage, IReadSideRepositoryWriter<InterviewStatuses> statuses)
        {
            this.dataExportWriter = dataExportWriter;
            this.users = userDocumentWriter;
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.statuses = statuses;
        }

        public override object[] Writers
        {
            get { return new object[] { dataExportWriter }; }
        }

        public void CleanWritersByEventSource(Guid eventSourceId)
        {
            this.dataExportWriter.DeleteInterview(eventSourceId);
        }

        public override object[] Readers
        {
            get { return new object[] { users, interviewSummaryStorage, statuses }; }
        }

        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            RecordAction(InterviewExportedAction.ApprovedByHeadquarter, evnt.Payload.UserId, evnt.EventSourceId, evnt.EventTimeStamp);
        }

        private void UpdateInterviewData(Guid interviewId, Guid userId, DateTime timeStamp, InterviewExportedAction action)
        {
            this.dataExportWriter.AddExportedDataByInterview(interviewId);

            RecordAction(action, userId, interviewId, timeStamp);
        }

        private void RecordAction(InterviewExportedAction action, Guid userId, Guid interviewId, DateTime timeStamp)
        {
            this.dataExportWriter.AddInterviewAction(action, interviewId, userId, timeStamp);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.SupervisorAssigned);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            RecordAction(InterviewExportedAction.InterviewerAssigned, evnt.Payload.UserId, evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            var interviewStatusHistory = this.statuses.GetById(evnt.EventSourceId);
            if (interviewStatusHistory == null)
                return;

            var recordedStatusesForInterview = interviewStatusHistory.InterviewCommentedStatuses.ToList();

            while (recordedStatusesForInterview.Any())
            {
                var lastStatusRecors = recordedStatusesForInterview.Last();
                if (listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded.Contains(lastStatusRecors.Status))
                    break;

                if (lastStatusRecors.Status == InterviewExportedAction.FirstAnswerSet)
                    this.dataExportWriter.AddInterviewAction(lastStatusRecors.Status, evnt.EventSourceId,
                        lastStatusRecors.StatusChangeOriginatorId, lastStatusRecors.Timestamp);
                
                recordedStatusesForInterview.Remove(lastStatusRecors);
            }

            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId,
                evnt.Payload.CompleteTime ?? evnt.EventTimeStamp, InterviewExportedAction.Completed);
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            RecordAction(InterviewExportedAction.Restarted, evnt.Payload.UserId, evnt.EventSourceId, evnt.Payload.RestartTime ?? evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewApproved> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.ApprovedBySupervisor);
        }

        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.RejectedBySupervisor);
        }

        public void Handle(IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            RecordAction(InterviewExportedAction.RejectedByHeadquarter, evnt.Payload.UserId, evnt.EventSourceId, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            this.dataExportWriter.DeleteInterview(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            this.dataExportWriter.DeleteInterview(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewRestored> evnt)
        {
            var interviewSummary = interviewSummaryStorage.GetById(evnt.EventSourceId);
            if (interviewSummary == null)
            {
                return;
            }

            var interviewStatusHistory = this.statuses.GetById(evnt.EventSourceId);
            if (interviewStatusHistory == null)
            {
                throw new NullReferenceException(string.Format("Missing interview status changes history for interview {0}", evnt.EventSourceId));
            }

            foreach (var interviewCommentedStatus in interviewStatusHistory.InterviewCommentedStatuses)
            {
                this.dataExportWriter.AddInterviewAction(interviewCommentedStatus.Status, evnt.EventSourceId, interviewCommentedStatus.StatusChangeOriginatorId, interviewCommentedStatus.Timestamp);
            }

            if (interviewStatusHistory.InterviewCommentedStatuses.Any() &&
                interviewStatusHistory.InterviewCommentedStatuses.Last().Status != InterviewExportedAction.Restored)
            {
                RecordAction(InterviewExportedAction.Restored, evnt.Payload.UserId, evnt.EventSourceId, evnt.EventTimeStamp);
            }
        }
    }
}
