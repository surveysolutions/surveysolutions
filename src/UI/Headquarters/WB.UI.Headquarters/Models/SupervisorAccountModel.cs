using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Models
{
    public class SupervisorAccountModel
    {
        [Required(ErrorMessageResourceType = typeof (SupervisorAccountResources), 
            ErrorMessageResourceName = "UserNameRequiredValidationMessage")]
        [MaxLength(40, ErrorMessageResourceType = typeof (SupervisorAccountResources), 
            ErrorMessageResourceName = "UserNameTooLongValidationMessage")]
        public string Login { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}