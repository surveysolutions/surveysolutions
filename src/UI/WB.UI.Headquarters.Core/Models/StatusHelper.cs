using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public static class StatusHelper
    {
        private static List<ComboboxViewItem> headquartersStatuses = null;

        private static readonly InterviewStatus[] headquarterStatusesList =
        {
            InterviewStatus.ApprovedByHeadquarters,
            InterviewStatus.ApprovedBySupervisor,
            InterviewStatus.Completed, 
            InterviewStatus.InterviewerAssigned, 
            InterviewStatus.RejectedByHeadquarters,
            InterviewStatus.RejectedBySupervisor,
            InterviewStatus.SupervisorAssigned,
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
            invisibleForUserStatuses.Concat(new[]
            {
                InterviewStatus.ApprovedBySupervisor,
                InterviewStatus.ApprovedByHeadquarters,
                InterviewStatus.Restarted,
            }).ToArray();

        public static IEnumerable<ComboboxViewItem> GetOnlyActualSurveyStatusViewItems(IAuthorizedUser authorizedUser)
        {
            if (authorizedUser.IsHeadquarter || authorizedUser.IsAdministrator)
            {
                if (headquartersStatuses == null)
                {
                    lock (invisibleForUserStatuses)
                    {
                        if (headquarterStatusesList == null)
                        {
                            headquartersStatuses = new List<ComboboxViewItem>
                            {
                                AsComboboxItem("AllExceptApprovedByHQ",
                                    Strings.AllInterviewersExceptApprovedByHeadquarters,
                                    headquarterStatusesList
                                        .Where(status => status != InterviewStatus.ApprovedByHeadquarters)
                                        .ToArray()
                                )
                            };

                            headquartersStatuses.AddRange(headquarterStatusesList.Select(status =>
                                AsComboboxItem(status, status)));
                        }
                    }
                }

                return headquartersStatuses;
            }

            var ignoreStatuses = authorizedUser.IsSupervisor
                ? invisibleForSupervisorStatuses
                : invisibleForUserStatuses;

            return Enum.GetValues(typeof(InterviewStatus))
                .OfType<InterviewStatus>()
                .Where(i => !ignoreStatuses.Contains(i))
                .Select(i => AsComboboxItem(i, i))
                .OrderBy(s => s.Key);
        }

        static ComboboxViewItem AsComboboxItem(InterviewStatus status, params InterviewStatus[] selector) =>
            AsComboboxItem(status.ToString().ToUpper(), status.ToLocalizeString(), selector);

        static ComboboxViewItem AsComboboxItem(string key, string translation, params InterviewStatus[] selector) =>
            new ComboboxViewItem(key, translation,
                JsonConvert.SerializeObject(selector.Select(s => s.ToString().ToUpper())));
    }
}
