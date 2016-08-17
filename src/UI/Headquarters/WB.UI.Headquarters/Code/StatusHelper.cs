using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public static class StatusHelper
    {
        private static readonly InterviewStatus[] invisibleForUserStatuses =
        {
            InterviewStatus.Created,
            InterviewStatus.ReadyForInterview,
            InterviewStatus.Restarted,
            InterviewStatus.SentToCapi,
            InterviewStatus.Restored,
            InterviewStatus.Deleted
        };

        private static readonly InterviewStatus[] invisibleForSupervisorStatuses =
            invisibleForUserStatuses.Concat( new[]
            { 
                InterviewStatus.ApprovedBySupervisor,
                InterviewStatus.ApprovedByHeadquarters,
            }).ToArray();

        internal static IEnumerable<SurveyStatusViewItem> GetAllSurveyStatusViewItems(InterviewStatus[] skipStatuses)
        {
            return from InterviewStatus status in Enum.GetValues(typeof(InterviewStatus))
                   where !skipStatuses.Contains(status)
                   select new SurveyStatusViewItem
                   {
                       Status = status,
                       StatusName = GetEnumDescription(status)
                   };
        }

        public static IEnumerable<SurveyStatusViewItem> GetOnlyActualSurveyStatusViewItems(IGlobalInfoProvider infoProvider)
        {
            var ignoreStatuses = infoProvider.IsSurepvisor
                ? invisibleForSupervisorStatuses
                : invisibleForUserStatuses;
            return GetAllSurveyStatusViewItems(ignoreStatuses)
                .OrderBy(status=>status.StatusName);
        }

        private static string GetEnumDescription(InterviewStatus value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof (DescriptionAttribute), false);

            return attributes.Length > 0
                       ? attributes[0].Description
                       : value.ToString();
        }
    }
}