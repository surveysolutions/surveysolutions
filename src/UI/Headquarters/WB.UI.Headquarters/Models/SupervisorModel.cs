using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Models
{
    public class SupervisorModel
    {
        public string SurveyTitle { get; set; }

        public string SurveyId{ get; set; }

        [Required(ErrorMessageResourceType = typeof (SupervisorAccountResources), 
            ErrorMessageResourceName = "UserNameRequiredValidationMessage")]
        [MaxLength(40, ErrorMessageResourceType = typeof (SupervisorAccountResources), 
            ErrorMessageResourceName = "UserNameTooLongValidationMessage")]
        public string Login { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(SupervisorAccountResources))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(SupervisorAccountResources))]
        [Compare("Password",  ErrorMessageResourceType = typeof (SupervisorAccountResources), 
            ErrorMessageResourceName = "UserPasswordsShouldMatchValidationMessage")]
        public string ConfirmPassword { get; set; }
    }
}