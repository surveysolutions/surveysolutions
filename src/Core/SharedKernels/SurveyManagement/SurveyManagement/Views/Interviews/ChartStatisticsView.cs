using System;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviews
{
    public class ChartStatisticsView
    {
        public object[][][] Lines { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}