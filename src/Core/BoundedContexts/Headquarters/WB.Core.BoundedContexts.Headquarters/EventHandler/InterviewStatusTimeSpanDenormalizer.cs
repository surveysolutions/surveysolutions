using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewStatusTimeSpanDenormalizer : BaseDenormalizer, IEventHandler<InterviewCompleted>,
                                            IEventHandler<InterviewApprovedByHQ>,
                                            IEventHandler<UnapprovedByHeadquarters>,
                                            IEventHandler<InterviewHardDeleted>
                        

    {
        private readonly IReadSideRepositoryWriter<InterviewStatusTimeSpans> interviewCustomStatusTimestampStorage;
        private readonly IReadSideRepositoryWriter<InterviewSummary> statuses;

        private readonly InterviewExportedAction[] beginStatusesForCompleteStatus = new[]
        {
            InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor
        };

        public InterviewStatusTimeSpanDenormalizer(
            IReadSideRepositoryWriter<InterviewStatusTimeSpans> interviewCustomStatusTimestampStorage,
            IReadSideRepositoryWriter<InterviewSummary> statuses)
        {
            this.interviewCustomStatusTimestampStorage = interviewCustomStatusTimestampStorage;
            this.statuses = statuses;
        }

        public override object[] Writers
        {
            get { return new[] { this.interviewCustomStatusTimestampStorage }; }
        }

        public override object[] Readers
        {
            get { return new object[] {this.statuses}; }
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            var statusHistory = this.statuses.GetById(evnt.EventSourceId);
            if (statusHistory == null)
                return;

            var lastAssignOfRejectStatus =
                statusHistory.InterviewCommentedStatuses.LastOrDefault(
                    s => this.beginStatusesForCompleteStatus.Contains(s.Status));

            if (lastAssignOfRejectStatus == null)
                return;
            var completeTime = evnt.Payload.CompleteTime ?? evnt.EventTimeStamp;
            var completeTimeSpan = completeTime - lastAssignOfRejectStatus.Timestamp;

            var interviewCustomStatusTimestamps = this.interviewCustomStatusTimestampStorage.GetById(evnt.EventSourceId);

            if (interviewCustomStatusTimestamps == null)
            {
                interviewCustomStatusTimestamps = new InterviewStatusTimeSpans()
                {
                    InterviewId = statusHistory.InterviewId.FormatGuid(),
                    QuestionnaireId = statusHistory.QuestionnaireId,
                    QuestionnaireVersion = statusHistory.QuestionnaireVersion
                };
            }

            var statusesAfterLastAssign = this.GetStausesAfterLastAssignOrReject(statusHistory, lastAssignOfRejectStatus);

            var wasInterviewRestarted =
                statusesAfterLastAssign.Any(s => s.Status == InterviewExportedAction.Restarted);

            if (wasInterviewRestarted)
            {
                this.ReplaceLastCompleteStatus(interviewCustomStatusTimestamps, lastAssignOfRejectStatus,
                   completeTime, completeTimeSpan);
            }
            else
                this.AddTimeSpanForCompleteStatus(interviewCustomStatusTimestamps, lastAssignOfRejectStatus,
                    completeTime, completeTimeSpan);

            this.interviewCustomStatusTimestampStorage.Store(interviewCustomStatusTimestamps,
                interviewCustomStatusTimestamps.InterviewId);
        }

        private InterviewCommentedStatus[] GetStausesAfterLastAssignOrReject(InterviewSummary statusHistory, InterviewCommentedStatus lastAssignOfRejectStatus)
        {
            return statusHistory.InterviewCommentedStatuses.SkipWhile(s => s.Id != lastAssignOfRejectStatus.Id)
                  .Skip(1)
                  .ToArray();
        }

        private void AddTimeSpanForCompleteStatus(InterviewStatusTimeSpans interviewCustomStatusTimestamps, InterviewCommentedStatus lastAssignOfRejectStatus, DateTime completeTime, TimeSpan timeSpan)
        {
            interviewCustomStatusTimestamps.TimeSpansBetweenStatuses.Add(
                new TimeSpanBetweenStatuses(lastAssignOfRejectStatus.SupervisorId,
                    lastAssignOfRejectStatus.InterviewerId, lastAssignOfRejectStatus.Status,
                    InterviewExportedAction.Completed,
                    completeTime, timeSpan,
                    lastAssignOfRejectStatus.SupervisorName,
                    lastAssignOfRejectStatus.InterviewerName));
        }

        private void ReplaceLastCompleteStatus(InterviewStatusTimeSpans interviewCustomStatusTimestamps, InterviewCommentedStatus lastAssignOfRejectStatus, DateTime completeTime, TimeSpan timeSpan)
        {
            var lastCompletedStatus =
             interviewCustomStatusTimestamps.TimeSpansBetweenStatuses.LastOrDefault(
                 s => s.EndStatus == InterviewExportedAction.Completed);
            if (lastCompletedStatus == null)
            {
                this.AddTimeSpanForCompleteStatus(interviewCustomStatusTimestamps, lastAssignOfRejectStatus, completeTime,
                    timeSpan);
                return;
            }
            lastCompletedStatus.EndStatusTimestamp = completeTime;
            lastCompletedStatus.TimeSpan = timeSpan;
        }
        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            var statusHistory = this.statuses.GetById(evnt.EventSourceId);
            if (statusHistory == null)
                return;

            var lastAssignOfRejectStatus = 
                statusHistory.InterviewCommentedStatuses.FirstOrDefault(
                    s =>
                        s.Status == InterviewExportedAction.InterviewerAssigned);

            if (lastAssignOfRejectStatus == null)
                return;

            var timeSpan = evnt.EventTimeStamp - lastAssignOfRejectStatus.Timestamp;

            var interviewCustomStatusTimestamps = this.interviewCustomStatusTimestampStorage.GetById(evnt.EventSourceId);

            if (interviewCustomStatusTimestamps == null)
            {
                interviewCustomStatusTimestamps = new InterviewStatusTimeSpans()
                {
                    InterviewId = statusHistory.InterviewId.FormatGuid(),
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

            this.interviewCustomStatusTimestampStorage.Store(interviewCustomStatusTimestamps, interviewCustomStatusTimestamps.InterviewId);
        }

        public void Handle(IPublishedEvent<UnapprovedByHeadquarters> evnt)
        {
           InterviewStatusTimeSpans interviewCustomStatusTimestamps = this.interviewCustomStatusTimestampStorage.GetById(evnt.EventSourceId);
            if (interviewCustomStatusTimestamps == null)
            {
                return;
            }

            List<TimeSpanBetweenStatuses> itemsToRemove = interviewCustomStatusTimestamps.TimeSpansBetweenStatuses.Where(
                    x => x.EndStatus == InterviewExportedAction.ApprovedByHeadquarter).ToList();

            if (itemsToRemove.Any())
            {
                foreach (var itemToRemove in itemsToRemove)
                {
                    interviewCustomStatusTimestamps.TimeSpansBetweenStatuses.Remove(itemToRemove);
                }
                
                this.interviewCustomStatusTimestampStorage.Store(interviewCustomStatusTimestamps,
                    interviewCustomStatusTimestamps.InterviewId);
            }
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            var interviewId = evnt.EventSourceId;
            interviewCustomStatusTimestampStorage.Remove(interviewId);
            statuses.Remove(interviewId);
        }
    }
}