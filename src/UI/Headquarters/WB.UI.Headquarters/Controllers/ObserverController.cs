using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Resources;
using Main.Core.Entities.SubEntities;
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
    [Authorize(Roles = "Administrator")]
    [ValidateInput(false)]
    public class ObserverController : TeamController
    {
        public ObserverController(ICommandService commandService, 
                              ILogger logger,
                              IIdentityManager identityManager)
            : base(commandService, logger, identityManager)
        {
            
        }

        public ActionResult Create()
        {
            this.ViewBag.ActivePage = MenuItem.Observers;

            return this.View(new UserModel());
        }

        [HttpPost]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Create(UserModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Observers;

            if (this.ModelState.IsValid)
            {
                try
                {
                    await this.CreateUserAsync(model, UserRoles.Observer);
                }
                catch (Exception e)
                {
                    this.Logger.Error(e.Message, e);
                    this.Error(e.Message);
                    return this.View(model);
                }
                
                this.Success(HQ.ObserverCreated);
                return this.RedirectToAction("Index");
            }

            return this.View(model);
        }

        
        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Observers;

            return this.View();
        }

        public async Task<ActionResult> Edit(string id)
        {
            this.ViewBag.ActivePage = MenuItem.Observers;

            var user = await this.identityManager.GetUserById(id);

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