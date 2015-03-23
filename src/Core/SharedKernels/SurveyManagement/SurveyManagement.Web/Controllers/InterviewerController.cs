using System;
using System.Web;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Shared.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
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
        public ActionResult Create(Guid supervisorId)
        {
            return this.View(new InterviewerModel() {SupervisorId = supervisorId});
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InterviewerModel model)
        {
            if (this.ModelState.IsValid)
            {
                UserView user = this.GetUserByName(model.UserName);
                if (user == null)
                {
                    this.CreateInterviewer(model, model.SupervisorId);
                    this.Success("Interviewer was successfully created");
                    return this.Back(model.SupervisorId);
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
            }

            return this.View(model);
        }

        [Authorize(Roles = "Administrator, Supervisor")]
        public ActionResult Index()
        {
            return this.View(this.GlobalInfo.GetCurrentUser().Id);
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
                    IsLocked = this.GlobalInfo.IsHeadquarter || this.GlobalInfo.IsAdministrator ? user.IsLockedByHQ : user.IsLockedBySupervisor,
                    UserName = user.UserName,
                    DevicesHistory = user.DeviceChangingHistory
                });
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserEditModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = this.GetUserById(model.Id);
                if (user != null)
                {
                    this.UpdateAccount(user: user, editModel: model);
                    this.Success(string.Format("Information about <b>{0}</b> successfully updated", user.UserName));
                    return this.Back(user.Supervisor.Id);
                }
                else
                {
                    this.Error("Could not update user information because current user does not exist");
                }
            }

            return this.View(model);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult Back(Guid id)
        {
            if (!(this.GlobalInfo.IsHeadquarter || this.GlobalInfo.IsAdministrator))
                return this.RedirectToAction("Index");

            var user = this.GetUserById(id);

            return this.RedirectToAction("Interviewers", "Supervisor",
                new {id = user.Supervisor == null ? user.PublicKey : user.Supervisor.Id});
        }
    }
}