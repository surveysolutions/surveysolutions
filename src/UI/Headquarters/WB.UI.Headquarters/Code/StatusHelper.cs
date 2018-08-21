using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public static class StatusHelper
    {
        private static readonly InterviewStatus[] invisibleForUserStatuses =
        {
            InterviewStatus.Created,
            InterviewStatus.ReadyForInterview,
            InterviewStatus.SentToCapi,
            InterviewStatus.Restored,
            InterviewStatus.Deleted,
            InterviewStatus.Restarted, 
        };

        private static readonly InterviewStatus[] invisibleForSupervisorStatuses =
            invisibleForUserStatuses.Concat( new[]
            { 
                InterviewStatus.ApprovedBySupervisor,
                InterviewStatus.ApprovedByHeadquarters,
                InterviewStatus.Restarted,
            }).ToArray();

        internal static IEnumerable<SurveyStatusViewItem> GetAllSurveyStatusViewItems(InterviewStatus[] skipStatuses)
        {
            return from InterviewStatus status in Enum.GetValues(typeof(InterviewStatus))
                   where !skipStatuses.Contains(status)
                   select new SurveyStatusViewItem
                   {
                       Status = status,
                       StatusName = status.ToLocalizeString()
                   };
        }

        public static IEnumerable<SurveyStatusViewItem> GetOnlyActualSurveyStatusViewItems(bool isSupervisor)
        {
            var ignoreStatuses = isSupervisor
                ? invisibleForSupervisorStatuses
                : invisibleForUserStatuses;
            return GetAllSurveyStatusViewItems(ignoreStatuses)
                .OrderBy(status=>status.StatusName);
        }
    }
}
