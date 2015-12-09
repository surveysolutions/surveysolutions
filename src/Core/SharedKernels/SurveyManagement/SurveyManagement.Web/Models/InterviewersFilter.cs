using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewersFilter
    {
        public Guid SupervisorId { get; set; }

        public bool? ShowOnlyNotConnectedToDevice { get; set; } 

        public bool ShowOnlyArchived { get; set; } 
    }
}