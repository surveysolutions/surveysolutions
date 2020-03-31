using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public static class StatusHelper
    {
        private static ComboboxViewItem ComboboxViewItem(string value, string title) => new ComboboxViewItem()
        {
            Key = value,
            Value = title,
        };

        private static readonly ComboboxViewItem[] headquartersStatuses =
        {
            ComboboxViewItem("All", Strings.AllInterviewers),
            ComboboxViewItem("AllExceptApprovedByHQ", Strings.AllInterviewersExceptApprovedByHeadquarters),
            ComboboxViewItem(InterviewStatus.ApprovedByHeadquarters.ToString(), Strings.InterviewStatus_ApprovedByHeadquarters),
            ComboboxViewItem(InterviewStatus.ApprovedBySupervisor.ToString(), Strings.InterviewStatus_ApprovedBySupervisor),
            ComboboxViewItem(InterviewStatus.Completed.ToString(), Strings.InterviewStatus_Completed),
            ComboboxViewItem(InterviewStatus.InterviewerAssigned.ToString(), Strings.InterviewStatus_InterviewerAssigned),
            ComboboxViewItem(InterviewStatus.RejectedByHeadquarters.ToString(), Strings.InterviewStatus_RejectedByHeadquarters),
            ComboboxViewItem(InterviewStatus.RejectedBySupervisor.ToString(), Strings.InterviewStatus_RejectedBySupervisor),
            ComboboxViewItem(InterviewStatus.SupervisorAssigned.ToString(), Strings.InterviewStatus_SupervisorAssigned),
        };

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

        internal static IEnumerable<ComboboxViewItem> GetAllSurveyStatusViewItems(InterviewStatus[] skipStatuses)
        {
            return from InterviewStatus status in Enum.GetValues(typeof(InterviewStatus))
                   where !skipStatuses.Contains(status)
                   select new ComboboxViewItem
                   {
                       Key = status.ToString(),
                       Value = status.ToLocalizeString(),
                   };
        }

        public static IEnumerable<ComboboxViewItem> GetOnlyActualSurveyStatusViewItems(IAuthorizedUser authorizedUser)
        {
            if (authorizedUser.IsHeadquarter || authorizedUser.IsAdministrator)
                return headquartersStatuses;

            var ignoreStatuses = authorizedUser.IsSupervisor
                ? invisibleForSupervisorStatuses
                : invisibleForUserStatuses;
            var statuses = GetAllSurveyStatusViewItems(ignoreStatuses)
                .OrderBy(status=>status.Key)
                .ToList();

            return statuses;
        }
    }
}
