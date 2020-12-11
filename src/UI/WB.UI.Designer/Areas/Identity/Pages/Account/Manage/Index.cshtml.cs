using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly SignInManager<DesignerIdentityUser> _signInManager;

        public IndexModel(
            UserManager<DesignerIdentityUser> userManager,
            SignInManager<DesignerIdentityUser> signInManager)
        {
            this.userManager = userManager;
            _signInManager = signInManager;
        }

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [StringLength(100, ErrorMessageResourceName = nameof(ErrorMessages.FullNameMaxLengthError), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
            public string? FullName { get; set; }

            [Required]
            [EmailAddress]
            public string? Email { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            var email = await userManager.GetEmailAsync(user);
            var fullName = await userManager.GetFullName(user.Id);

            Input = new InputModel
            {
                Email = email,
                FullName = fullName
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input == null || !ModelState.IsValid)
            {
                return Page();
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }

            var email = await userManager.GetEmailAsync(user);
            if (Input.Email != email)
            {
                var setEmailResult = await userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    this.ErrorMessage = setEmailResult.Errors.First().Description;
                    return RedirectToPage();
                }
            }

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
                        await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, Input.FullName!));
                    }
                    else
                    {
                        await userManager.ReplaceClaimAsync(user, existingFullName,
                            new Claim(ClaimTypes.Name, Input.FullName!));
                    }
                }
            }

            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = AccountResources.AccountInfoUpdated;
            return RedirectToPage();
        }
    }
}
