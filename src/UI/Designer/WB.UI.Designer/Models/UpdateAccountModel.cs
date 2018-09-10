using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    public class UpdateAccountModel
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email", Order = 2)]
        public string Email { get; set; }

        [Display(Name = "Is Approved", Order = 3)]
        public bool IsApproved { get; set; }

        [Display(Name = "Is Locked Out", Order = 4)]
        public bool IsLockedOut { get; set; }

        public bool CanImportOnHq { get; set; }

        [Display(Name = "User name", Order = 1)]
        public string UserName { get; set; }

        [Display(Name = "Full name", Order = 2)]
        public string FullName { get; set; }
    }
}
