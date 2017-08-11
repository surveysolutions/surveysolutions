using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using Main.Core.Entities.SubEntities;
using Resources;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    [ValidateInput(false)]
    public class ApiUserController : TeamController
    {
        public ApiUserController(ICommandService commandService,
            ILogger logger,
            IAuthorizedUser authorizedUser,
            HqUserManager userManager)
            : base(commandService, logger, authorizedUser, userManager)
        {
        }

        public ActionResult Create()
        {
            this.ViewBag.ActivePage = MenuItem.ApiUsers;

            return this.View(new UserModel
            {
                HideDetails = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Create(UserModel model)
        {
            this.ViewBag.ActivePage = MenuItem.ApiUsers;

            if (this.ModelState.IsValid)
            {
                var creationResult = await this.CreateUserAsync(model, UserRoles.ApiUser);
                if (creationResult.Succeeded)
                {
                    this.Success(Pages.Profile_ApiUserWasCreated.FormatString(model.UserName));
                    return this.RedirectToAction("Index");
                }
                AddErrors(creationResult);
            }

            return this.View(model);
        }

        public async Task<ActionResult> Edit(Guid id)
        {
            this.ViewBag.ActivePage = MenuItem.ApiUsers;

            var user = await this.userManager.FindByIdAsync(id);

            if (user == null) throw new HttpException(404, string.Empty);
            if (!user.IsInRole(UserRoles.ApiUser)) throw new HttpException(403, HQ.NoPermission);

            return this.View(new UserEditModel
            {
                UserName = user.UserName,
                Id = user.Id,
                Email = user.Email,
                IsLocked = user.IsLockedByHeadquaters,
                HideDetails = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserEditModel model)
        {
            this.ViewBag.ActivePage = MenuItem.ApiUsers;

            if (ModelState.IsValid)
            {
                var updateResult = await this.UpdateAccountAsync(model);
                if (updateResult.Succeeded)
                {
                    this.Success(Strings.HQ_AccountController_AccountUpdatedSuccessfully.FormatString(model.UserName));
                    return this.RedirectToAction("Index");
                }
                AddErrors(updateResult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.ApiUsers;

            return this.View();
        }
    }
}