using System;
using System.Web.Mvc;
using Core.Supervisor.Views.Interviewer;
using Core.Supervisor.Views.User;
using Main.Core.Commands.User;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    using System.Web;

    public class TeamController : BaseController
    {
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;

        public TeamController(ICommandService commandService, 
                              IGlobalInfoProvider globalInfo, 
                              ILogger logger,
                              IViewFactory<UserViewInputModel, UserView> userViewFactory,
                              IViewFactory<UserListViewInputModel, UserListView> userListViewFactory,
                              IViewFactory<InterviewersInputModel, InterviewersView> interviewersViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.userViewFactory = userViewFactory;
            this.ViewBag.ActivePage = MenuItem.Teams;
        }

        [Authorize(Roles = "Headquarter")]
        public ActionResult AddInterviewer(Guid id)
        {
            return this.View(new InterviewerViewModel {Id = id});
        }

        [HttpPost]
        [Authorize(Roles = "Headquarter")]
        public ActionResult AddInterviewer(InterviewerViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                UserView user =
                    this.userViewFactory.Load(
                        new UserViewInputModel(UserName: model.Name, UserEmail: null));
                if (user == null)
                {
                    this.CommandService.Execute(new CreateUserCommand(
                                                    publicKey: Guid.NewGuid(),
                                                    userName: model.Name,
                                                    password: SimpleHash.ComputeHash(model.Password),
                                                    email: model.Email,
                                                    isLocked: false,
                                                    roles: new[] {UserRoles.Operator},
                                                    supervsor: this.GetUser(model.Id).GetUseLight()));
                    this.Success("Interviewer was successfully created");
                    return this.RedirectToAction("Interviewers", new {id = model.Id});
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
            }

            return this.View(model);
        }

        [Authorize(Roles = "Headquarter")]
        public ActionResult AddSupervisor()
        {
            return this.View(new SupervisorViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Headquarter")]
        public ActionResult AddSupervisor(SupervisorViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                UserView user =
                    this.userViewFactory.Load(
                        new UserViewInputModel(UserName: model.Name, UserEmail: null));
                if (user == null)
                {
                    this.CommandService.Execute(new CreateUserCommand(
                                                    publicKey: Guid.NewGuid(),
                                                    userName: model.Name,
                                                    password: SimpleHash.ComputeHash(model.Password),
                                                    email: model.Email,
                                                    isLocked: false,
                                                    roles: new[] {UserRoles.Supervisor},
                                                    supervsor: null));

                    this.Success("Supervisor was successfully created");
                    return this.RedirectToAction("Index");
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
            }

            return this.View(model);
        }

        [Authorize(Roles = "Headquarter")]
        public ActionResult Index()
        {
            return this.View();
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult Interviewers(Guid? id)
        {
            if (this.GlobalInfo.IsHeadquarter && !id.HasValue)
                return this.RedirectToAction("Index");

            return this.View(id);
        }

        private UserView GetUser(Guid id)
        {
            return this.userViewFactory.Load(new UserViewInputModel(id));
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult Details(Guid id)
        {
            var user = this.GetUser(id);

            if(user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserEditModel()
                {
                    Id = user.PublicKey,
                    Email = user.Email,
                    IsLocked = user.IsLocked,
                    UserName = user.UserName
                });
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        [HttpPost]
        public ActionResult Details(UserEditModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = this.GetUser(model.Id);
                if (user != null)
                {
                    this.CommandService.Execute(
                        new ChangeUserCommand()
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
                    this.Success(string.Format("Information about <b>{0}</b> sucessfully updated", user.UserName));
                    return this.DetailsBackByUser(user);
                }
                else
                {
                    this.Error("Could not update user information because current user does not exist");
                }
            }

            return this.View(model);
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult DetailsBack(Guid id)
        {
            var user = this.GetUser(id);
            return this.DetailsBackByUser(user);
        }

        private ActionResult DetailsBackByUser(UserView user)
        {
            return this.GlobalInfo.IsHeadquarter && user.Supervisor == null
                       ? this.RedirectToAction("Index")
                       : this.RedirectToAction("Interviewers", new {id = user.Supervisor.Id});
        }
    }
}