using System;
using System.ComponentModel.DataAnnotations;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewerModel : UserModel
    {
        [Key]
        public Guid SupervisorId { get; set; }
    }
}