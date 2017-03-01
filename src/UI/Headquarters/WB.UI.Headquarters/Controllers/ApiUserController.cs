using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    [ValidateInput(false)]
    public class ApiUserController : TeamController
    {
        public ApiUserController(ICommandService commandService,
            ILogger logger,
            IIdentityManager identityManager)
            : base(commandService, logger, identityManager)
        {
        }

        public ActionResult Create()
        {
            this.ViewBag.ActivePage = MenuItem.ApiUsers;

            return this.View(new UserModel());
        }

        [HttpPost]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Create(UserModel model)
        {
            this.ViewBag.ActivePage = MenuItem.ApiUsers;

            if (this.ModelState.IsValid)
            {
                try
                {
                    await this.CreateUserAsync(model, UserRoles.ApiUser);
                }
                catch (Exception e)
                {
                    this.Logger.Error(e.Message, e);
                    this.Error(e.Message);
                    return this.View(model);
                }

                this.Success("API User was successfully created");
                return this.RedirectToAction("Index");
            }

            return this.View(model);
        }

        public async Task<ActionResult> Edit(string id)
        {
            this.ViewBag.ActivePage = MenuItem.ApiUsers;

            var user = await this.identityManager.GetUserById(id);

            if (user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserEditModel
            {
                UserName = user.UserName,
                Id = user.Id,
                Email = user.Email,
                IsLocked = user.IsLockedByHeadquaters
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
                    this.Success($"Information about <b>{model.UserName}</b> successfully updated");
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