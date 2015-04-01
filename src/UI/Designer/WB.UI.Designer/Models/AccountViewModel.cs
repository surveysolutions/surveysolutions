using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    [DisplayName("Account")]
    public class AccountViewModel
    {
        [Display(Name = "Comment", Order = 12)]
        public string Comment { get; set; }

        [Display(Name = "Created date", Order = 3)]
        public string CreationDate { get; set; }

        [Display(Name = "Email", Order = 2)]
        public string Email { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Is Approved", Order = 5)]
        public bool IsApproved { get; set; }

        [Display(Name = "Is Locked Out", Order = 6)]
        public bool IsLockedOut { get; set; }

        [Display(Name = "Last Lockout Date", Order = 9)]
        public string LastLockoutDate { get; set; }

        [Display(Name = "Last login", Order = 4)]
        public string LastLoginDate { get; set; }

        [Display(Name = "Last Password Changed Date", Order = 10)]
        public string LastPasswordChangedDate { get; set; }

        [Display(Name = "Questionnaires", Order = 13)]
        public IEnumerable<QuestionnaireListViewModel> Questionnaires { get; set; }

        [Display(Name = "Name", Order = 1)]
        public string UserName { get; set; }
    }
}