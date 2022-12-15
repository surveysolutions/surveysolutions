using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewStatusTimeSpanDenormalizer : 
        ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>,
        IUpdateHandler<InterviewSummary, InterviewCompleted>,
        IUpdateHandler<InterviewSummary, InterviewApprovedByHQ>,
        IUpdateHandler<InterviewSummary, UnapprovedByHeadquarters>,
        IUpdateHandler<InterviewSummary, InterviewHardDeleted>
    {
        private static readonly InterviewExportedAction[] beginStatusesForCompleteStatus = new[]
        {
            InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor
        };

        private InterviewCommentedStatus[] GetStatusesAfterLastAssignOrReject(InterviewSummary statusHistory, InterviewCommentedStatus lastAssignOfRejectStatus)
        {
            return statusHistory.InterviewCommentedStatuses.SkipWhile(s => s.Id != lastAssignOfRejectStatus.Id)
                  .Skip(1)
                  .ToArray();
        }

        private void AddTimeSpanForCompleteStatus(InterviewSummary interviewSummary, InterviewCommentedStatus lastAssignOfRejectStatus, DateTime completeTime, TimeSpan timeSpan)
        {
            interviewSummary.TimeSpansBetweenStatuses.Add(
                new TimeSpanBetweenStatuses(lastAssignOfRejectStatus.SupervisorId,
                    lastAssignOfRejectStatus.InterviewerId, lastAssignOfRejectStatus.Status,
                    InterviewExportedAction.Completed,
                    completeTime, timeSpan,
                    lastAssignOfRejectStatus.SupervisorName,
                    lastAssignOfRejectStatus.InterviewerName)
                {
                    InterviewSummary = interviewSummary
                });
        }

        private void ReplaceLastCompleteStatus(InterviewSummary interviewSummary, InterviewCommentedStatus lastAssignOfRejectStatus, DateTime completeTime, TimeSpan timeSpan)
        {
            var lastCompletedStatus = interviewSummary.TimeSpansBetweenStatuses.LastOrDefault(s => s.EndStatus == InterviewExportedAction.Completed);
            if (lastCompletedStatus == null)
            {
                this.AddTimeSpanForCompleteStatus(interviewSummary, lastAssignOfRejectStatus, completeTime, timeSpan);
                return;
            }
            lastCompletedStatus.EndStatusTimestamp = completeTime;
            lastCompletedStatus.TimeSpan = timeSpan;
        }
      
        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewCompleted> evnt)
        {
            var lastAssignOfRejectStatus = state.InterviewCommentedStatuses.LastOrDefault(s => beginStatusesForCompleteStatus.Contains(s.Status));

            if (lastAssignOfRejectStatus == null)
                return state;

            var completeTime = evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.CompleteTime ?? evnt.EventTimeStamp;
            var completeTimeSpan = completeTime - lastAssignOfRejectStatus.Timestamp;

            var statusesAfterLastAssign = this.GetStatusesAfterLastAssignOrReject(state, lastAssignOfRejectStatus);

            var wasInterviewRestarted = statusesAfterLastAssign.Any(s => s.Status == InterviewExportedAction.Restarted);

            if (wasInterviewRestarted)
                this.ReplaceLastCompleteStatus(state, lastAssignOfRejectStatus, completeTime, completeTimeSpan);
            else
                this.AddTimeSpanForCompleteStatus(state, lastAssignOfRejectStatus, completeTime, completeTimeSpan);

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            var lastAssignOfRejectStatus = state.InterviewCommentedStatuses.FirstOrDefault(s => s.Status == InterviewExportedAction.InterviewerAssigned);

            if (lastAssignOfRejectStatus == null)
                return state;

            var timeSpan = evnt.EventTimeStamp - lastAssignOfRejectStatus.Timestamp;

            state.TimeSpansBetweenStatuses.Add(
                new TimeSpanBetweenStatuses(
                    lastAssignOfRejectStatus.SupervisorId,
                    lastAssignOfRejectStatus.InterviewerId, 
                    lastAssignOfRejectStatus.Status,
                    InterviewExportedAction.ApprovedByHeadquarter,
                    evnt.EventTimeStamp, 
                    timeSpan, 
                    lastAssignOfRejectStatus.SupervisorName,
                    lastAssignOfRejectStatus.InterviewerName)
                {
                    InterviewSummary = state
                });

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<UnapprovedByHeadquarters> evnt)
        {
            List<TimeSpanBetweenStatuses> itemsToRemove = state.TimeSpansBetweenStatuses.Where(x => x.EndStatus == InterviewExportedAction.ApprovedByHeadquarter).ToList();

            if (itemsToRemove.Any())
            {
                foreach (var itemToRemove in itemsToRemove)
                {
                    state.TimeSpansBetweenStatuses.Remove(itemToRemove);
                }
            }

            return state;
        }

        public InterviewSummary Update(InterviewSummary state, IPublishedEvent<InterviewHardDeleted> @event)
        {
            return null;
        }
    }
}
