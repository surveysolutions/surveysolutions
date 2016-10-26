using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [ValidateInput(false)]
    public class InterviewerController : TeamController
    {
        public InterviewerController(ICommandService commandService, 
                              ILogger logger,
                              IIdentityManager identityManager)
            : base(commandService, logger, identityManager)
        {
        }


        [Authorize(Roles = "Administrator, Headquarter")]
        public async Task<ActionResult> Create(Guid? supervisorId)
        {
            if (!supervisorId.HasValue)
                return this.View(new InterviewerModel() { IsShowSupervisorSelector = true });

            var supervisor = await this.identityManager.GetUserByIdAsync(supervisorId.Value);

            if (supervisor == null) throw new HttpException(404, string.Empty);

            return this.View(new InterviewerModel() {SupervisorId = supervisorId.Value, SupervisorName = supervisor.UserName});
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Create(InterviewerModel model)
        {
            if (ModelState.IsValid)
            {
                var creationResult = await this.CreateUserAsync(model, UserRoles.Interviewer, model.SupervisorId);
                if (creationResult.Succeeded)
                {
                    await this.identityManager.SignInAsync(model.UserName, model.Password, isPersistent: false);

                    this.Success(Pages.InterviewerController_InterviewerCreationSuccess);
                    return this.Back();
                }
                AddErrors(creationResult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public async Task<ActionResult> Edit(Guid id)
        {
            var user = await this.identityManager.GetUserByIdAsync(id);

            if(user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserEditModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    IsLocked = user.IsLockedByHeadquaters,
                    IsLockedBySupervisor = user.IsLockedBySupervisor,
                    UserName = user.UserName,
                    //DevicesHistory = user.DeviceChangingHistory.ToList(),
                    PersonName = user.FullName,
                    PhoneNumber = user.PhoneNumber
                });
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Edit(UserEditModel model)
        {
            if (ModelState.IsValid)
            {
                var updateResult = await this.UpdateAccountAsync(model);
                if (updateResult.Succeeded)
                {
                    this.Success(string.Format(Pages.InterviewerController_EditSuccess, model.UserName));
                    return this.Back();
                }
                AddErrors(updateResult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult Back()
        {
            return this.RedirectToAction("Index", "Interviewers");
        }
    }
}