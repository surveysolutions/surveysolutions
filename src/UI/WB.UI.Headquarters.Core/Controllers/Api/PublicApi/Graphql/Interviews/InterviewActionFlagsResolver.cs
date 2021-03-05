using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public class InterviewActionFlagsResolver
    {
        public List<InterviewActionFlags> GetActionFlags([Parent] InterviewSummary interviewSummary, [Service] IAuthorizedUser user)
        {
            IEnumerable<InterviewActionFlags> GetFlags()
            {
                if (user.IsInterviewer)
                {
                    if ((interviewSummary.Status == InterviewStatus.Created
                         || interviewSummary.Status == InterviewStatus.SupervisorAssigned
                         || interviewSummary.Status == InterviewStatus.InterviewerAssigned
                         || interviewSummary.Status == InterviewStatus.SentToCapi) 
                        && !interviewSummary.ReceivedByInterviewerAtUtc.HasValue 
                        && !interviewSummary.WasCompleted)
                    {
                        yield return InterviewActionFlags.CanBeDeleted;
                    }

                    if (interviewSummary.Status == InterviewStatus.Completed)
                    {
                        yield return InterviewActionFlags.CanBeRestarted;
                    }

                    if (interviewSummary.Status != InterviewStatus.Completed)
                    {
                        yield return InterviewActionFlags.CanBeOpened;
                    }

                    yield break;
                }

                if (user.IsSupervisor)
                {
                    if (interviewSummary.Status == InterviewStatus.Created
                        || interviewSummary.Status == InterviewStatus.SupervisorAssigned
                        || interviewSummary.Status == InterviewStatus.InterviewerAssigned
                        || interviewSummary.Status == InterviewStatus.RejectedBySupervisor
                        || interviewSummary.Status == InterviewStatus.WebInterview)
                    {
                        yield return InterviewActionFlags.CanBeReassigned;
                    }

                    if (interviewSummary.Status == InterviewStatus.Completed 
                        || interviewSummary.Status == InterviewStatus.RejectedByHeadquarters
                        || interviewSummary.Status == InterviewStatus.RejectedBySupervisor)
                    {
                        yield return InterviewActionFlags.CanBeApproved;
                    }

                    if (interviewSummary.Status == InterviewStatus.Completed
                        || interviewSummary.Status == InterviewStatus.RejectedByHeadquarters)
                    {
                        yield return InterviewActionFlags.CanBeRejected;
                    }
                    
                    yield break;
                }

                if ((interviewSummary.Status == InterviewStatus.Created
                     || interviewSummary.Status == InterviewStatus.SupervisorAssigned
                     || interviewSummary.Status == InterviewStatus.InterviewerAssigned
                     || interviewSummary.Status == InterviewStatus.SentToCapi) 
                    && !interviewSummary.ReceivedByInterviewerAtUtc.HasValue 
                    && !interviewSummary.WasCompleted)
                    yield return InterviewActionFlags.CanBeDeleted;

                if (interviewSummary.Status == InterviewStatus.ApprovedBySupervisor 
                    || interviewSummary.Status == InterviewStatus.Completed
                    || interviewSummary.Status == InterviewStatus.RejectedBySupervisor)
                    yield return InterviewActionFlags.CanBeApproved;

                if (interviewSummary.Status == InterviewStatus.ApprovedBySupervisor 
                    || interviewSummary.Status == InterviewStatus.Completed)
                    yield return InterviewActionFlags.CanBeRejected;

                if (interviewSummary.Status == InterviewStatus.ApprovedByHeadquarters)
                    yield return InterviewActionFlags.CanBeUnApprovedByHq;

                if (interviewSummary.Status == InterviewStatus.SupervisorAssigned
                    || interviewSummary.Status == InterviewStatus.InterviewerAssigned
                    || interviewSummary.Status == InterviewStatus.Completed
                    || interviewSummary.Status == InterviewStatus.RejectedBySupervisor
                    || interviewSummary.Status == InterviewStatus.RejectedByHeadquarters)
                    yield return InterviewActionFlags.CanBeReassigned;
            }

            return GetFlags().ToList();
        }
    }
}
