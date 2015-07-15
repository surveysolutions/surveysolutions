using System.Linq;
using Main.Core.Events.Sync;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    internal class InterviewStatusTimeSpanDenormalizer : BaseDenormalizer, IEventHandler<InterviewCompleted>,
                                            IEventHandler<InterviewApprovedByHQ>
    {
        private readonly IReadSideRepositoryWriter<InterviewStatusTimeSpans> interviewCustomStatusTimestampStorage;
        private readonly IReadSideRepositoryWriter<InterviewStatuses> statuses;

        private readonly InterviewExportedAction[] beginStatusesForCompleteStatus = new[]
        {
            InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor,
            InterviewExportedAction.Restarted
        };

        public InterviewStatusTimeSpanDenormalizer(
            IReadSideRepositoryWriter<InterviewStatusTimeSpans> interviewCustomStatusTimestampStorage,
            IReadSideRepositoryWriter<InterviewStatuses> statuses)
        {
            this.interviewCustomStatusTimestampStorage = interviewCustomStatusTimestampStorage;
            this.statuses = statuses;
        }

        public override object[] Writers
        {
            get { return new[] { interviewCustomStatusTimestampStorage }; }
        }

        public override object[] Readers
        {
            get { return new object[] {statuses}; }
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            var statusHistory = statuses.GetById(evnt.EventSourceId);
            if(statusHistory==null)
                return;

            var lastAssignOfRejectStatus =
                statusHistory.InterviewCommentedStatuses.LastOrDefault(
                    s => beginStatusesForCompleteStatus.Contains(s.Status));

            if (lastAssignOfRejectStatus == null)
                return;

            var timeSpan = (evnt.Payload.CompleteTime ?? evnt.EventTimeStamp) - lastAssignOfRejectStatus.Timestamp;

            var interviewCustomStatusTimestamps = interviewCustomStatusTimestampStorage.GetById(evnt.EventSourceId);

            if (interviewCustomStatusTimestamps == null)
            {
                interviewCustomStatusTimestamps = new InterviewStatusTimeSpans()
                {
                    InterviewId = statusHistory.InterviewId,
                    QuestionnaireId = statusHistory.QuestionnaireId,
                    QuestionnaireVersion = statusHistory.QuestionnaireVersion
                };
            }

            interviewCustomStatusTimestamps.TimeSpansBetweenStatuses.Add(
                new TimeSpanBetweenStatuses(lastAssignOfRejectStatus.SupervisorId,
                    lastAssignOfRejectStatus.InterviewerId, lastAssignOfRejectStatus.Status,
                    InterviewExportedAction.Completed,
                    evnt.Payload.CompleteTime ?? evnt.EventTimeStamp, timeSpan, lastAssignOfRejectStatus.SupervisorName,
                    lastAssignOfRejectStatus.InterviewerName));

            interviewCustomStatusTimestampStorage.Store(interviewCustomStatusTimestamps, interviewCustomStatusTimestamps.InterviewId);
        }

        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            var statusHistory = statuses.GetById(evnt.EventSourceId);
            if (statusHistory == null)
                return;

            var lastAssignOfRejectStatus =
                statusHistory.InterviewCommentedStatuses.FirstOrDefault(
                    s =>
                        s.Status == InterviewExportedAction.InterviewerAssigned);

            if (lastAssignOfRejectStatus == null)
                return;

            var timeSpan = evnt.EventTimeStamp - lastAssignOfRejectStatus.Timestamp;

            var interviewCustomStatusTimestamps = interviewCustomStatusTimestampStorage.GetById(evnt.EventSourceId);

            if (interviewCustomStatusTimestamps == null)
            {
                interviewCustomStatusTimestamps = new InterviewStatusTimeSpans()
                {
                    InterviewId = statusHistory.InterviewId,
                    QuestionnaireId = statusHistory.QuestionnaireId,
                    QuestionnaireVersion = statusHistory.QuestionnaireVersion
                };
            }

            interviewCustomStatusTimestamps.TimeSpansBetweenStatuses.Add(
                new TimeSpanBetweenStatuses(lastAssignOfRejectStatus.SupervisorId,
                    lastAssignOfRejectStatus.InterviewerId, lastAssignOfRejectStatus.Status,
                    InterviewExportedAction.ApprovedByHeadquarter,
                    evnt.EventTimeStamp, timeSpan, lastAssignOfRejectStatus.SupervisorName,
                    lastAssignOfRejectStatus.InterviewerName));

            interviewCustomStatusTimestampStorage.Store(interviewCustomStatusTimestamps, interviewCustomStatusTimestamps.InterviewId);
        }
    }
}