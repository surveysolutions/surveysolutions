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
            var x = interviewSummary;
            IEnumerable<InterviewActionFlags> GetFlags()
            {
                if (user.IsInterviewer)
                {
                    if ((x.Status == InterviewStatus.Created
                         || x.Status == InterviewStatus.SupervisorAssigned
                         || x.Status == InterviewStatus.InterviewerAssigned
                         || x.Status == InterviewStatus.SentToCapi) &&
                        !x.ReceivedByInterviewer && !x.WasCompleted)
                    {
                        yield return InterviewActionFlags.CanBeDeleted;
                    }

                    if (x.Status == InterviewStatus.Completed)
                    {
                        yield return InterviewActionFlags.CanBeRestarted;
                    }

                    if (x.Status != InterviewStatus.Completed)
                    {
                        yield return InterviewActionFlags.CanBeOpened;
                    }

                    yield break;
                }

                if (user.IsSupervisor)
                {
                    if (x.Status == InterviewStatus.Created
                        || x.Status == InterviewStatus.SupervisorAssigned
                        || x.Status == InterviewStatus.InterviewerAssigned
                        || x.Status == InterviewStatus.RejectedBySupervisor)
                    {
                        yield return InterviewActionFlags.CanBeReassigned;
                    }

                    if (x.Status == InterviewStatus.Completed 
                        || x.Status == InterviewStatus.RejectedByHeadquarters)
                    {
                        yield return InterviewActionFlags.CanBeApproved;
                    }

                    if (x.Status == InterviewStatus.Completed
                        || x.Status == InterviewStatus.RejectedByHeadquarters)
                    {
                        yield return InterviewActionFlags.CanBeRejected;
                    }
                    
                    yield break;
                }

                if ((x.Status == InterviewStatus.Created
                     || x.Status == InterviewStatus.SupervisorAssigned
                     || x.Status == InterviewStatus.InterviewerAssigned
                     || x.Status == InterviewStatus.SentToCapi) &&
                    !x.ReceivedByInterviewer && !x.WasCompleted)
                    yield return InterviewActionFlags.CanBeDeleted;

                if (x.Status == InterviewStatus.ApprovedBySupervisor || x.Status == InterviewStatus.Completed)
                    yield return InterviewActionFlags.CanBeApproved;

                if (x.Status == InterviewStatus.ApprovedBySupervisor || x.Status == InterviewStatus.Completed)
                    yield return InterviewActionFlags.CanBeRejected;

                if (x.Status == InterviewStatus.ApprovedByHeadquarters)
                    yield return InterviewActionFlags.CanBeUnApprovedByHq;

                if (x.Status == InterviewStatus.SupervisorAssigned
                    || x.Status == InterviewStatus.InterviewerAssigned
                    || x.Status == InterviewStatus.Completed
                    || x.Status == InterviewStatus.RejectedBySupervisor
                    || x.Status == InterviewStatus.RejectedByHeadquarters)
                    yield return InterviewActionFlags.CanBeReassigned;
            }

            return GetFlags().ToList();
        }
    }
}
