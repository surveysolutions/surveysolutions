using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    [DisplayName("Account")]
    public class DeleteAccountModel
    {
        [Key]
        public string Id { get; set; }

        [Display(Name = "User name")]
        public string UserName { get; set; }
        
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}