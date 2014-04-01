using System;
using System.Web;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Team.Models;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.UI.Headquarters.Models.Team;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class TeamController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;

        public TeamController(ICommandService commandService,
            IViewFactory<UserViewInputModel, UserView> userViewFactory)
        {
            this.commandService = commandService;
            this.userViewFactory = userViewFactory;
        }

        public ActionResult AddInterviewer(Guid id)
        {
            return this.View(new UserCreateModel { Id = id });
        }

        [HttpPost]
        public ActionResult AddInterviewer(UserCreateModel model)
        {
            if (this.ModelState.IsValid)
            {
                UserView user = this.userViewFactory.Load(new UserViewInputModel(model.UserName, null));
                if (user == null)
                {
                    this.commandService.Execute(new CreateUserCommand(Guid.NewGuid(), model.UserName, SimpleHash.ComputeHash(model.Password),
                        model.Email,
                        isLocked: model.IsLocked,
                        roles: new[] { UserRoles.Operator },
                        supervsor: this.GetUser(model.Id).GetUseLight()));
                    this.Success("Interviewer was successfully created");
                    return this.RedirectToAction("Interviewers", new { id = model.Id });
                }
                this.Error("User name already exists. Please enter a different user name.");
            }

            return this.View(model);
        }

        public ActionResult AddSupervisor()
        {
            return this.View(new UserCreateModel());
        }

        [HttpPost]
        public ActionResult AddSupervisor(UserCreateModel model)
        {
            if (this.ModelState.IsValid)
            {
                UserView user =
                    this.userViewFactory.Load(
                        new UserViewInputModel(model.UserName, null));
                if (user == null)
                {
                    this.commandService.Execute(new CreateUserCommand(Guid.NewGuid(), model.UserName, SimpleHash.ComputeHash(model.Password),
                        model.Email,
                        isLocked: model.IsLocked,
                        roles: new[] { UserRoles.Supervisor },
                        supervsor: null));

                    this.Success("Supervisor was successfully created");
                    return this.RedirectToAction("Index");
                }
                this.Error("User name already exists. Please enter a different user name.");
            }

            return this.View(model);
        }

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult Interviewers(Guid? id)
        {
            return this.View(id);
        }

        private UserView GetUser(Guid id)
        {
            return this.userViewFactory.Load(new UserViewInputModel(id));
        }

        public ActionResult Details(Guid id)
        {
            UserView user = this.GetUser(id);

            if (user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserViewModel
            {
                Id = user.PublicKey,
                Email = user.Email,
                IsLocked = user.IsLocked,
                UserName = user.UserName
            });
        }

        [HttpPost]
        public ActionResult Details(UserViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                UserView user = this.GetUser(model.Id);
                if (user != null)
                {
                    this.commandService.Execute(
                        new ChangeUserCommand
                        {
                            PublicKey = user.PublicKey,
                            PasswordHash = string.IsNullOrEmpty(model.Password)
                                ? user.Password
                                : SimpleHash.ComputeHash(model.Password),
                            Email = model.Email,
                            IsLocked = model.IsLocked,
                            Roles = user.Roles.ToArray(),
                        }
                        );
                    this.Success(string.Format("Information about <b>{0}</b> successfully updated", user.UserName));
                    return this.RedirectToAction("Index");
                }
                this.Error("Could not update user information because current user does not exist");
            }

            return this.View(model);
        }
    }
}