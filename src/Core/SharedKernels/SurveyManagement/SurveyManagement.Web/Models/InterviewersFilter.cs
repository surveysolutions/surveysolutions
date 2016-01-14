using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewersFilter
    {
        public Guid SupervisorId { get; set; }

        public bool? ConnectedToDevice { get; set; } 

        public bool Archived { get; set; } 
    }
}