using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Core.Supervisor.Views.Survey
{
    using System;

    public class SurveyStatusViewItem
    {
        public InterviewStatus Status { get; set; }

        public string StatusName { get; set; }
    }
}