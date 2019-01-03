using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public class SurveysAndStatusesReportInputModel : ListViewModelBase
    {
        public string TeamLeadName { get; set; }
        public string ResponsibleName { get; set; }
        public Guid? QuestionnaireId { get; set; }
    }
}
