using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    /// <summary>
    /// Define LogOn model
    /// </summary>
    public class LogOn2faModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; }
    }
}
