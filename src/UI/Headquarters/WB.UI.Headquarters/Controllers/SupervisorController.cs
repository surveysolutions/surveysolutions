using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Resources;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{

    [LimitsFilter]
    [ValidateInput(false)]
    [Authorize(Roles = "Administrator, Headquarter, Observer")]
    public class SupervisorController : TeamController
    {
        public SupervisorController(ICommandService commandService, 
                              ILogger logger,
                              IAuthorizedUser authorizedUser,
                              HqUserManager userManager)
            : base(commandService, logger, authorizedUser, userManager)
        {
        }

        public ActionResult Create()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowed]
        public async Task<ActionResult> Create(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var creationResult = await this.CreateUserAsync(model, UserRoles.Supervisor);

                if (creationResult.Succeeded)
                {
                    this.Success(HQ.SuccessfullyCreated);
                    return this.RedirectToAction("Index");
                }
                AddErrors(creationResult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public ActionResult Index()
        {
            return this.View();
        }

        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public ActionResult Archived()
        {
            return this.View();
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public async Task<ActionResult> Edit(Guid id)
        {
            var user = await this.userManager.FindByIdAsync(id);

            if(user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserEditModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    IsLocked = user.IsLockedByHeadquaters,
                    UserName = user.UserName,
                    PersonName = user.FullName,
                    PhoneNumber = user.PhoneNumber
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowed]
        public async Task<ActionResult> Edit(UserEditModel model)
        {
            if (ModelState.IsValid)
            {
                var creationResult = await this.UpdateAccountAsync(model);
                if (creationResult.Succeeded)
                {
                    this.Success(string.Format(HQ.UserWasUpdatedFormat, model.UserName));
                    return this.RedirectToAction("Index");
                }
                AddErrors(creationResult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}