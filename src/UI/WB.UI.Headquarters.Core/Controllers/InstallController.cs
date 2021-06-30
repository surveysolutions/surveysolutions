using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.UI.Headquarters.Controllers
{
    public class InstallController : Controller
    {
        private readonly ISupportedVersionProvider supportedVersionProvider;
        private readonly SignInManager<HqUser> signInManager;
        private readonly UserManager<HqUser> userManager;
        private readonly IUserRepository userRepository;
        
        public InstallController(ISupportedVersionProvider supportedVersionProvider,
                                 SignInManager<HqUser> identityManager,
                                 UserManager<HqUser> userManager,
                                 IUserRepository userRepository)
        {
            this.userManager = userManager;
            this.userRepository = userRepository;
            this.supportedVersionProvider = supportedVersionProvider;
            this.signInManager = identityManager;
        }

        public IActionResult Finish()
        {
            var isExistAnyUser = this.userRepository.Users.Any();
            if (isExistAnyUser)
                return Forbid();

            return View(new FinishIntallationModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finish(FinishIntallationModel model)
        {
            var isExistAnyUser = this.userRepository.Users.Any();
            if (isExistAnyUser)
                return Forbid(); 

            if (ModelState.IsValid)
            {
                var hqUser = new HqUser
                {
                    Id = Guid.Parse(@"00000000000040000000000000000001"),
                    FullName = @"Administrator",
                    UserName = model.UserName,
                    Email = model.Email,
                    PasswordChangeRequired = false
                };
                var creationResult = await this.userManager.CreateAsync(hqUser, model.Password);

                if (creationResult.Succeeded)
                {
                    await this.userManager.AddToRoleAsync(hqUser, UserRoles.Administrator.ToString());
                    this.supportedVersionProvider.RememberMinSupportedVersion();

                    return this.Redirect("~/");
                }

                foreach (IdentityError error in creationResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}
