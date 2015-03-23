using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    public class AccountListViewItemModel : ActionItem
    {
        [Display(Name = "Created date", Order = 4)]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Email", Order = 2)]
        public string Email { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Approved?", Order = 5)]
        public bool IsApproved { get; set; }

        [Display(Name = "Locked?", Order = 5)]
        public bool IsLockedOut { get; set; }

        [Display(Name = "Last login", Order = 3)]
        public string LastLoginDate { get; set; }

        [Display(Name = "Name", Order = 1)]
        [Default]
        public string UserName { get; set; }

        public override bool CanCopy
        {
            get
            {
                return false;
            }
        }

        public override bool CanExport
        {
            get
            {
                return false;
            }
        }
    }
}