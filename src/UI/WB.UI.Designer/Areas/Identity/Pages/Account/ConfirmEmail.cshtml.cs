using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Designer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<DesignerIdentityUser> _userManager;

        public ConfirmEmailModel(UserManager<DesignerIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public string? UserId { get; set; }

        [BindProperty]
        public string? Code { get; set; }

        public bool AutoSubmit { get; private set; }

        public IActionResult OnGet(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            this.UserId = userId;
            this.Code = code;
            this.AutoSubmit = true;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UserId == null || Code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{UserId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, Code);
            if (!result.Succeeded)
            {
                var error = result.Errors.First();
                TempData[Alerts.ERROR] = error.Description;
                return Page();
            }

            TempData[Alerts.SUCCESS] = ErrorMessages.Your_email_is_verified;

            return RedirectToPage("Login");
        }
    }
}
