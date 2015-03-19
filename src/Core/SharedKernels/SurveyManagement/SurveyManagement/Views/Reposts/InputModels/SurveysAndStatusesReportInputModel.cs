using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class SurveysAndStatusesReportInputModel : ListViewModelBase
    {
        public Guid? UserId { get; set; }

        public Guid? ViewerId { get; set; }
    }
}