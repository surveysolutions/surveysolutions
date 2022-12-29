using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Areas.Admin.Pages
{
    [Authorize(Roles = nameof(SimpleRoleEnum.Administrator))]
    public class UserDetailsModel : PageModel
    {
        private readonly UserManager<DesignerIdentityUser> users;
        private readonly IQuestionnaireHelper questionnaireHelper;

        public UserDetailsModel(UserManager<DesignerIdentityUser> users,
            IQuestionnaireHelper questionnaireHelper)
        {
            this.users = users ?? throw new ArgumentNullException(nameof(users));
            this.questionnaireHelper = questionnaireHelper ?? throw new ArgumentNullException(nameof(questionnaireHelper));
        }

        [DisplayName("Account")]
        public class AccountViewModel
        {
            [Display(Name = "Comment", Order = 12)]
            public string? Comment { get; set; }

            [Display(Name = "Created date", Order = 3)]
            public string? CreationDate { get; set; }

            [Display(Name = "Email", Order = 2)]
            public string? Email { get; set; }

            [Key]
            public Guid Id { get; set; }

            [Display(Name = "Is Approved", Order = 6)]
            public bool IsApproved { get; set; }

            [Display(Name = "Is Locked Out", Order = 7)]
            public bool IsLockedOut { get; set; }

            [Display(Name = "Can import on HQ", Order = 5)]
            public bool CanImportOnHq { get; set; }

            [Display(Name = "Last Password Changed Date", Order = 10)]
            public string? LastPasswordChangedDate { get; set; }

            [Display(Name = "Questionnaires", Order = 13)]
            public IEnumerable<QuestionnaireListViewModel> OwnedQuestionnaires { get; set; } = new List<QuestionnaireListViewModel>();

            [Display(Name = "Questionnaires", Order = 14)]
            public IEnumerable<QuestionnaireListViewModel> SharedQuestionnaires { get; set; } = new List<QuestionnaireListViewModel>();

            [Display(Name = "Name", Order = 1)] public string UserName { get; set; } = string.Empty;

            [Display(Name = "Full name", Order = 2)]
            public string? FullName { get; set; }
        }

        public AccountViewModel Account { get; private set; } = new AccountViewModel();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            this.Account = new AccountViewModel();
            
            var account = await this.users.FindByIdAsync(id.FormatGuid());
            if (account == null) return NotFound();

            var ownedQuestionnaires = this.questionnaireHelper.GetMyQuestionnairesByViewerId(account,
                isAdmin: User.IsAdmin(), folderId: null);

            var sharedQuestionnaires = this.questionnaireHelper.GetSharedQuestionnairesByViewer(account,
                isAdmin: User.IsAdmin(), folderId: null);

            ownedQuestionnaires.ToList().ForEach(x =>
            {
                x.CanEdit = false;
                x.CanDelete = false;
            });

            sharedQuestionnaires.ToList().ForEach(x =>
            {
                x.CanEdit = false;
                x.CanDelete = false;
            });

            this.OwnedQuestionnaires = ownedQuestionnaires;
            this.SharedQuestionnaires = sharedQuestionnaires;

            var accountViewModel = new AccountViewModel
            {
                Id = account.Id,
                CreationDate = account.CreatedAtUtc.ToUIString(),
                Email = account.Email,
                IsApproved = account.EmailConfirmed,
                IsLockedOut = account.LockoutEnd.HasValue && account.LockoutEnd.Value >= DateTimeOffset.UtcNow ,
                CanImportOnHq = account.CanImportOnHq,
                UserName = account.UserName ?? String.Empty,
                OwnedQuestionnaires = ownedQuestionnaires,
                SharedQuestionnaires = sharedQuestionnaires,
                FullName = await this.users.GetFullName(account.Id)
            };
            this.Account = accountViewModel;

            return Page();
        }

        public IPagedList<QuestionnaireListViewModel>? SharedQuestionnaires { get; set; }

        public IPagedList<QuestionnaireListViewModel>? OwnedQuestionnaires { get; set; }
    }
}
