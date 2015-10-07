using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewersModel
    {
        public Guid SupervisorId { get; set; }
        public string UserName { get; set; }
        public bool ShowOnlyNotConnectedToDevice { get; set; } 
    }
}