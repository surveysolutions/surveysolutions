using System;
using System.Web;
using System.Web.Mvc;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class InterviewerController : TeamController
    {
        public InterviewerController(ICommandService commandService, 
                              IGlobalInfoProvider globalInfo, 
                              ILogger logger,
                              IViewFactory<UserViewInputModel, UserView> userViewFactory)
            : base(commandService, globalInfo, logger, userViewFactory)
        {
        }

        [Authorize(Roles = "Headquarter")]
        public ActionResult Create(Guid supervisorId)
        {
            return this.View(new InterviewerModel() {SupervisorId = supervisorId});
        }

        [HttpPost]
        [Authorize(Roles = "Headquarter")]
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

        [Authorize(Roles = "Supervisor")]
        public ActionResult Index()
        {
            return this.View(this.GlobalInfo.GetCurrentUser().Id);
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult Edit(Guid id)
        {
            var user = this.GetUserById(id);

            if(user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserEditModel()
                {
                    Id = user.PublicKey,
                    Email = user.Email,
                    IsLocked = this.GlobalInfo.IsHeadquarter ? user.IsLockedByHQ : user.IsLockedBySupervisor,
                    UserName = user.UserName
                });
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        [HttpPost]
        public ActionResult Edit(UserEditModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = this.GetUserById(model.Id);
                if (user != null)
                {
                    this.UpdateSupervisorOrInterviewer(user: user, editModel: model);
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

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult Back(Guid id)
        {
            if(!this.GlobalInfo.IsHeadquarter)
                return this.RedirectToAction("Index");

            var user = this.GetUserById(id);

            return this.RedirectToAction("Interviewers", "Supervisor",
                new {id = user.Supervisor == null ? user.PublicKey : user.Supervisor.Id});
        }
    }
}