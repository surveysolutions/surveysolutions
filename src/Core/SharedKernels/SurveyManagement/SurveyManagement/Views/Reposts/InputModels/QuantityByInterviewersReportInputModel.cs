using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class QuantityByInterviewersReportInputModel : QuantityBySupervisorsReportInputModel
    {
        public QuantityByInterviewersReportInputModel():base()
        {
        }

        public Guid SupervisorId { get; set; }
    }
}