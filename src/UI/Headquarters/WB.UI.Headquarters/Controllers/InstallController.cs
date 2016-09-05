using System.Threading.Tasks;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    public class InstallController : BaseController
    {
        private readonly ISupportedVersionProvider supportedVersionProvider;
        private readonly IIdentityManager identityManager;

        public InstallController(ICommandService commandService,
                                 ISupportedVersionProvider supportedVersionProvider,
                                 ILogger logger,
                                 IIdentityManager identityManager)
            : base(commandService, logger)
        {
            this.supportedVersionProvider = supportedVersionProvider;
            this.identityManager = identityManager;
        }

        public ActionResult Finish()
        {
            return View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PreventDoubleSubmit]
        public async Task<ActionResult> Finish(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var creationResult = await this.identityManager.CreateUserAsync(new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email
                }, model.Password, UserRoles.Administrator);

                if (creationResult.Succeeded)
                {
                    await this.identityManager.SignInAsync(model.UserName, model.Password, isPersistent: false);

                    this.supportedVersionProvider.RememberMinSupportedVersion();

                    return this.RedirectToAction("Index", "Headquarters");
                }
                AddErrors(creationResult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}