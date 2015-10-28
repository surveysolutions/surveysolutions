using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewersFilter
    {
        public Guid Id { get; set; }

        public bool ShowOnlyNotConnectedToDevice { get; set; } 
    }
}