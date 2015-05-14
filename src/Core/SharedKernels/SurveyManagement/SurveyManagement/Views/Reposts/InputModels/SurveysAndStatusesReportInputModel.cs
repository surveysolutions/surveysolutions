using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class SurveysAndStatusesReportInputModel : ListViewModelBase
    {
        public Guid? TeamLeadId { get; set; }

        public Guid? ResponsibleId { get; set; }
    }
}