using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    /// <summary>
    /// Define LogOn model
    /// </summary>
    public class LogOnModel
    {
        /// <summary>
        /// Gets or sets UserName.
        /// </summary>
        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredUserNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [Display(ResourceType = typeof(FieldsAndValidations), Name = nameof(FieldsAndValidations.LogOnModel_UserName))]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets Password.
        /// </summary>
        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredPasswordErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(FieldsAndValidations), Name = nameof(FieldsAndValidations.LogOnModel_Password))]
        public string Password { get; set; }

    }
}