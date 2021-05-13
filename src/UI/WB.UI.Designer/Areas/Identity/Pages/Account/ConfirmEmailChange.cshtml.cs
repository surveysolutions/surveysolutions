using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Designer.Areas.Identity.Pages.Account
{
    public class ConfirmEmailChangeModel : PageModel
    {
        private readonly UserManager<DesignerIdentityUser> _userManager;

        public ConfirmEmailChangeModel(UserManager<DesignerIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGet(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var tokenIsValid = await _userManager.VerifyUserTokenAsync(user,
                _userManager.Options.Tokens.EmailConfirmationTokenProvider,
                UserManager<DesignerIdentityUser>.ConfirmEmailTokenPurpose, 
                code);

            if (tokenIsValid)
            {
                await _userManager.SetEmailAsync(user, user.PendingEmail);

                user.EmailConfirmed = true;
                user.PendingEmail = null;
                await _userManager.UpdateAsync(user);
            }

            TempData[Alerts.SUCCESS] = ErrorMessages.Your_email_is_verified;

            return RedirectToPage("Login");
        }
    }
}
