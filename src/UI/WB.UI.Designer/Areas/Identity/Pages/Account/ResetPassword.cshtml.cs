using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<DesignerIdentityUser> _userManager;

        public ResetPasswordModel(UserManager<DesignerIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty] public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required(ErrorMessageResourceType = typeof(ErrorMessages),
                ErrorMessageResourceName = nameof(ErrorMessages.Password_required))]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare(nameof(Password), ErrorMessageResourceType = typeof(ErrorMessages),
                ErrorMessageResourceName = nameof(ErrorMessages.The_password_and_confirmation_password_do_not_match))]
            public string ConfirmPassword { get; set; } = string.Empty;

            public string Code { get; set; } = String.Empty;
            public string UserId { get; set; } = String.Empty;
        }

        public async Task<IActionResult> OnGet(string? code = null, string? userId = null)
        {
            if (code == null || userId == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null) return BadRequest("Provided user id is invalid");

                Input = new InputModel
                {
                    Code = code,
                    UserId = userId
                };

                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input == null || !ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByIdAsync(Input.UserId);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            user.PasswordSalt = null;
            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
