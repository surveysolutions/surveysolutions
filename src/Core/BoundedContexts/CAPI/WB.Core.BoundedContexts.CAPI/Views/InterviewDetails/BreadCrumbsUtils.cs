using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    internal class BreadCrumbsUtils
    {
        public static IEnumerable<InterviewItemId> CloneBreadcrumbs(IEnumerable<InterviewItemId> originalBreadcrumbs, decimal[] propagationVector)
        {
            var breadcrumbs = originalBreadcrumbs.ToArray();

            var lastVector = new decimal[propagationVector.Length];
            propagationVector.CopyTo(lastVector, 0);

            for (int i = breadcrumbs.Length - 1; i >= 0; i--)
            {
                var newVector = lastVector;

                if (EmptyBreadcrumbForRosterRow.IsInterviewItemIdEmptyBreadcrumbForRosterRow(breadcrumbs[i]))
                {
                    if (lastVector.Length > 0)
                        lastVector = lastVector.Take(lastVector.Length - 1).ToArray();
                }

                breadcrumbs[i] = new InterviewItemId(breadcrumbs[i].Id, newVector);
            }
            return breadcrumbs;
        }
    }

    internal static class EmptyBreadcrumbForRosterRow
    {
        public static InterviewItemId CreateEmptyBreadcrumbForRosterRow(Guid rosterId)
        {
            return new InterviewItemId(rosterId, new decimal[] { -1 });
        }

        public static bool IsInterviewItemIdEmptyBreadcrumbForRosterRow(InterviewItemId interviewItemId)
        {
            return interviewItemId.InterviewItemPropagationVector != null && interviewItemId.InterviewItemPropagationVector.Length == 1 &&
                interviewItemId.InterviewItemPropagationVector[0] == -1;
        }
    }
}
