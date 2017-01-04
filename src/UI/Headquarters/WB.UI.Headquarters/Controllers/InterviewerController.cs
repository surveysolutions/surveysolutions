using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [ValidateInput(false)]
    public class InterviewerController : TeamController
    {
        public InterviewerController(ICommandService commandService, 
                              IGlobalInfoProvider globalInfo, 
                              ILogger logger,
                              IUserViewFactory userViewFactory,
                              IPasswordHasher passwordHasher)
            : base(commandService, globalInfo, logger, userViewFactory, passwordHasher)
        {
        }


        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult Create(Guid? supervisorId)
        {
            if (!supervisorId.HasValue)
                return this.View(new InterviewerModel() { IsShowSupervisorSelector = true });

            var supervisor = this.GetUserById(supervisorId.Value);

            if (supervisor == null) throw new HttpException(404, string.Empty);

            return this.View(new InterviewerModel() {SupervisorId = supervisorId.Value, SupervisorName = supervisor.UserName});
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult Create(InterviewerModel model)
        { 
            if (this.ModelState.IsValid)
            {
                try
                {
                    this.CreateInterviewer(model, model.SupervisorId);
                }
                catch (Exception e)
                {
                    this.Logger.Error(e.Message, e);
                    this.Error(e.Message);
                    return this.View(model);
                }
             
                this.Success(Pages.InterviewerController_InterviewerCreationSuccess);
                return this.Back();
            }

            return this.View(model);
        }


        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult Edit(Guid id)
        {
            var user = this.GetUserById(id);

            if(user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserEditModel
                {
                    Id = user.PublicKey,
                    Email = user.Email,
                    IsLocked = user.IsLockedByHQ,
                    IsLockedBySupervisor = user.IsLockedBySupervisor,
                    UserName = user.UserName,
                    DevicesHistory = user.DeviceChangingHistory.ToList(),
                    PersonName = user.PersonName,
                    PhoneNumber = user.PhoneNumber
                });
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult Edit(UserEditModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = this.GetUserById(model.Id);
                if (user == null)
                {
                    this.Error(Pages.InterviewerController_UpdateUseFailure);
                }

                this.UpdateAccount(user: user, editModel: model);
                
                this.Success(string.Format(Pages.InterviewerController_EditSuccess, user.UserName));
                return this.Back();
            }
           
            return this.View(model);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult Back()
        {
            return this.RedirectToAction("Index", "Interviewers");
        }
    }
}