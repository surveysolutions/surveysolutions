using System;
using System.ComponentModel.DataAnnotations;
using WB.Core.SharedKernels.SurveyManagement.Web.Properties;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewerModel : UserModel
    {
        [Key]
        [Required(ErrorMessageResourceName = "RequiredSupervisorErrorMessage", ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [Display(Name = "SupervisorFieldName", ResourceType = typeof(FieldsAndValidations), Order = 0)]
        public Guid SupervisorId { get; set; }

        public string SupervisorName { get; set; }

        public bool IsShowSupervisorSelector { get; set; }
    }
}