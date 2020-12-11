using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Areas.Admin.Pages
{
    [Authorize(Roles = nameof(SimpleRoleEnum.Administrator))]
    public class EditUserModel : PageModel
    {
        private readonly UserManager<DesignerIdentityUser> userManager;

        public EditUserModel(UserManager<DesignerIdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        [TempData]
        public string? Message { get; set; }
        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Key]
            public Guid Id { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email", Order = 2)]
            public string? Email { get; set; }

            [Display(Name = "Is Approved", Order = 3)]
            public bool IsApproved { get; set; }

            [Display(Name = "Is Locked Out", Order = 4)]
            public bool IsLockedOut { get; set; }

            public bool CanImportOnHq { get; set; }

            [Display(Name = "User name", Order = 1)]
            public string UserName { get; set; } = String.Empty;

            [Display(Name = "Full name", Order = 2)]
            [StringLength(RegisterModel.FullNameMaxLength, ErrorMessageResourceName = nameof(ErrorMessages.FullNameMaxLengthError), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
            public string? FullName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var user = await this.userManager.FindByIdAsync(id.FormatGuid());
            if (user == null) return NotFound();

            this.Input = new InputModel
            {
                Id = id,
                Email = user.Email,
                IsApproved = user.EmailConfirmed,
                IsLockedOut = user.LockoutEnabled && user.LockoutEnd.HasValue,
                CanImportOnHq = user.CanImportOnHq,
                UserName = user.UserName,
                FullName = await userManager.GetFullName(id)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (Input!= null && ModelState.IsValid)
            {
                var user = await this.userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                if (!user.Email.Equals(Input.Email, StringComparison.InvariantCultureIgnoreCase))
                {
                    var emailChanged = await userManager.SetEmailAsync(user, Input.Email);
                    if (!emailChanged.Succeeded)
                    {
                        this.ErrorMessage = emailChanged.Errors.First().Description;
                        return RedirectToPage(new {id = id});
                    }
                }

                user.EmailConfirmed = Input.IsApproved;

                user.LockoutEnabled = Input.IsLockedOut;
                user.LockoutEnd = Input.IsLockedOut ? DateTimeOffset.MaxValue : (DateTimeOffset?)null;
                user.CanImportOnHq = Input.CanImportOnHq;

                var claims = await userManager.GetClaimsAsync(user);
                var existingFullName = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
                if (existingFullName?.Value != Input.FullName)
                {
                    if (string.IsNullOrWhiteSpace(Input.FullName) && existingFullName != null)
                    {
                        await userManager.RemoveClaimAsync(user, existingFullName);
                    }
                    else
                    {
                        if (existingFullName == null)
                        {
                            await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, 
                                Input.FullName!));
                        }
                        else
                        {
                            await userManager.ReplaceClaimAsync(user, existingFullName,
                                new Claim(ClaimTypes.Name, Input.FullName!));
                        }
                    }
                }

                this.Message = "Account updated";

                await this.userManager.UpdateAsync(user);

                if (Input.IsLockedOut)
                    await userManager.UpdateSecurityStampAsync(user);
            }

            return RedirectToPage(new { id = id });
        }
    }
}
