using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
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
        [Required]
        [Display(ResourceType = typeof(FieldsAndValidations), Name = nameof(FieldsAndValidations.LogOnModel_UserName))]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets Password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(FieldsAndValidations), Name = nameof(FieldsAndValidations.LogOnModel_Password))]
        public string Password { get; set; }

        public bool RequireCaptcha { get; set; } = false;

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }
    }
}
