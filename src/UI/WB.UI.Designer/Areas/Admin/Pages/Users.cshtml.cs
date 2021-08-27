using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Areas.Admin.Pages
{
    [Authorize(Roles = nameof(SimpleRoleEnum.Administrator))]
    public class UsersModel : PageModel
    {
        private readonly IAccountListViewFactory accountListViewFactory;
        private readonly UserManager<DesignerIdentityUser> userManager;

        public UsersModel(IAccountListViewFactory accountListViewFactory,
            UserManager<DesignerIdentityUser> userManager)
        {
            this.accountListViewFactory = accountListViewFactory ?? throw new ArgumentNullException(nameof(accountListViewFactory));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task OnGet(int? p, string sb, int? so, string f)
        {
            var claims = this.User.Claims.ToList();

            int page = p ?? 1;

            this.PageIndex = p;
            this.SortBy = sb;
            this.Filter = f;
            this.SortOrder = so;

            if (so.ToBool())
            {
                sb = $"{sb} Desc";
            }
            var users = this.accountListViewFactory.Load(new AccountListViewInputModel
            {
                Filter = f,
                Page = page,
                PageSize = GlobalHelper.GridPageItemsCount,
                Order = sb ?? string.Empty,
            });

            async Task<bool> EditAction(DesignerIdentityUser user)
            {
                return await this.userManager.IsInRoleAsync(user, SimpleRoleEnum.Administrator.ToString());
            }

            

            List<AccountListViewItemModel> retVal = new List<AccountListViewItemModel>();

            foreach (var identityUser in users.Items)
            {
                var canEdit = await EditAction(identityUser);
                var fullName = await this.userManager.GetFullName(identityUser.Id);
                var uiItem = new AccountListViewItemModel
                {
                    Id = identityUser.Id,
                    UserName = identityUser.UserName,
                    Email = identityUser.Email,
                    CreationDate = identityUser.CreatedAtUtc,
                    IsApproved = identityUser.EmailConfirmed,
                    IsLockedOut = identityUser.LockoutEnabled && identityUser.LockoutEnd.HasValue && identityUser.LockoutEnd.Value >= DateTimeOffset.UtcNow,
                    LockoutEnd = (!identityUser.LockoutEnabled && identityUser.LockoutEnd.HasValue && identityUser.LockoutEnd.Value >= DateTimeOffset.UtcNow) ? identityUser.LockoutEnd : null,
                    CanEdit = canEdit,
                    CanOpen = false,
                    CanDelete = false,
                    CanPreview = canEdit,
                    CanImportOnHq = identityUser.CanImportOnHq,
                    FullName = fullName
                };
                retVal.Add(uiItem);
            }


            this.List = retVal.ToPagedList(page, GlobalHelper.GridPageItemsCount, users.TotalCount);
        }

        public int? SortOrder { get; set; }

        public string? Filter { get; set; }

        public string? SortBy { get; set; }

        public int? PageIndex { get; set; }

        public IPagedList<AccountListViewItemModel>? List { get; private set; }
    }

    public class AccountListViewItemModel : ActionItem
    {
        [Display(Name = "Created date", Order = 4)]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Email", Order = 2)]
        public string Email { get; set; } = String.Empty;

        [Display(Name = "FullName", Order = 2)]
        public string? FullName { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Approved?", Order = 5)]
        public bool IsApproved { get; set; }

        [Display(Name = "Manually Locked?", Order = 5)]
        public bool IsLockedOut { get; set; }

        [Display(Name = "Lockout end")]
        public DateTimeOffset? LockoutEnd { get; set; }
        
        [Display(Name = "Can import on HQ")]
        public bool CanImportOnHq { get; set; }

        [Display(Name = "Name", Order = 1)]
        [Default]
        public string? UserName { get; set; }

        public override bool CanCopy => false;

        public override bool CanExport => false;
    }

}
