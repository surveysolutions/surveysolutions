using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    [DisplayName("Account")]
    public class AccountViewModel
    {
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Name")]
        public string UserName { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Created date")]
        public DateTime CreationDate { get; set; }
        [Display(Name = "Last login")]
        public DateTime LastLoginDate { get; set; }
        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; }
        [Display(Name = "Is Locked Out")]
        public bool IsLockedOut { get; set; }
        [Display(Name = "Is Online")]
        public bool IsOnline { get; set; }
        [Display(Name = "Last Activity Date")]
        public DateTime LastActivityDate { get; set; }
        [Display(Name = "Last Lockout Date")]
        public DateTime LastLockoutDate { get; set; }
        [Display(Name = "Last Password Changed Date")]
        public DateTime LastPasswordChangedDate { get; set; }
        [Display(Name = "Password Question")]
        public string PasswordQuestion { get; set; }
        [Display(Name = "Comment")]
        public string Comment { get; set; }
    }
}