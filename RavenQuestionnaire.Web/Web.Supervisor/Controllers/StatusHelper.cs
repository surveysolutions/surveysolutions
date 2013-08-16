using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Core.Supervisor.Views.Survey;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Web.Supervisor.Controllers
{
    public static class StatusHelper {
        internal static IEnumerable<SurveyStatusViewItem> SurveyStatusViewItems()
        {
            return from InterviewStatus status in Enum.GetValues(typeof (InterviewStatus))
                   where status != InterviewStatus.Restarted
                   select new SurveyStatusViewItem()
                       {
                           Status = status,
                           StatusName = GetEnumDescription(status)
                       };
        }

        private static string GetEnumDescription(InterviewStatus value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0
                       ? attributes[0].Description
                       : value.ToString();
        }
    }
}