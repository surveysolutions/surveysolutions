using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Core.Supervisor.Views.Survey;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Web.Supervisor.Controllers
{
    public static class StatusHelper
    {
        private static readonly InterviewStatus[] invisibleForUserStatuses =
        {
            InterviewStatus.Created,
            InterviewStatus.ReadyForInterview,
            InterviewStatus.Restarted,
            InterviewStatus.SentToCapi,
            InterviewStatus.Deleted
        };

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

        internal static IEnumerable<SurveyStatusViewItem> GetOnlyActualSurveyStatusViewItems()
        {
            return from InterviewStatus status in Enum.GetValues(typeof (InterviewStatus))
                   where !invisibleForUserStatuses.Contains(status)
                   select new SurveyStatusViewItem
                       {
                           Status = status,
                           StatusName = GetEnumDescription(status)
                       };
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