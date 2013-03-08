using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    public class AccountListViewItemModel
    {
        [Key]
        public string Id { get; set; }
        [Display(Name = "Name")]
        public string UserName { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Created date")]
        public string CreationDate { get; set; }
        [Display(Name = "Last login")]
        public string LastLoginDate { get; set; }
        [Display(Name = "Approved?")]
        public bool IsApproved { get; set; }
        [Display(Name = "Locked?")]
        public bool IsLockedOut { get; set; }
    }
}