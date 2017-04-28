using System;
using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewerModel : UserModel
    {
        [Key]
        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredSupervisorErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [Display(Name = nameof(FieldsAndValidations.SupervisorFieldName), ResourceType = typeof(FieldsAndValidations), Order = 0)]
        public Guid SupervisorId { get; set; }

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredSupervisorErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [Display(Name = nameof(FieldsAndValidations.SupervisorFieldName), ResourceType = typeof(FieldsAndValidations), Order = 0)]
        public string SupervisorName { get; set; }

        public bool IsShowSupervisorSelector { get; set; }

        [Display(Name = nameof(FieldsAndValidations.IsLockedBySupervisorFieldName), ResourceType = typeof(FieldsAndValidations), Order = 7)]
        public bool IsLockedBySupervisor { get; set; }
    }
}