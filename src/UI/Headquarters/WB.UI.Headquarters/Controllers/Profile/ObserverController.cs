using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Resources;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeOr403(Roles = "Administrator")]
    [ValidateInput(false)]
    public class ObserverController : TeamController
    {
        public ObserverController(ICommandService commandService, 
                              ILogger logger,
                              IAuthorizedUser authorizedUser,
                              HqUserManager userManager)
            : base(commandService, logger, authorizedUser, userManager)
        {
            
        }

        public ActionResult Create()
        {
            this.ViewBag.ActivePage = MenuItem.Observers;

            return this.View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Create(UserModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Observers;

            if (this.ModelState.IsValid)
            {
                var creationResult = await this.CreateUserAsync(model, UserRoles.Observer);
                if (creationResult.Succeeded)
                {
                    this.Success(HQ.ObserverCreatedFormat.FormatString(model.UserName));
                    return this.RedirectToAction("Index");
                }
                AddErrors(creationResult);
            }

            return this.View(model);
        }

        
        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Observers;

            return this.View();
        }

        public async Task<ActionResult> Edit(Guid id)
        {
            this.ViewBag.ActivePage = MenuItem.Observers;

            var user = await this.userManager.FindByIdAsync(id);

            if(user == null) throw new HttpException(404, string.Empty);
            if (!user.IsInRole(UserRoles.Observer)) throw new HttpException(403, HQ.NoPermission);

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
        public async Task<ActionResult> Edit(UserEditModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Observers;

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