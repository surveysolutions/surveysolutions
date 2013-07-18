using Main.Core.Utility;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web.Mvc;

    using Core.Supervisor.Views.Interviewer;
    using Core.Supervisor.Views.User;

    using Main.Core.Commands.User;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Models;

    public class TeamController : BaseController
    {
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;

        public TeamController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IViewFactory<UserViewInputModel, UserView> userViewFactory, IViewFactory<UserListViewInputModel, UserListView> userListViewFactory, IViewFactory<InterviewersInputModel, InterviewersView> interviewersViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.userViewFactory = userViewFactory;
            this.ViewBag.ActivePage = MenuItem.Teams;
        }

        [Authorize(Roles = "Headquarter")]
        public ActionResult AddInterviewer(Guid id)
        {
            return this.View(new InterviewerViewModel { Id = id });
        }

        [HttpPost]
        [Authorize(Roles = "Headquarter")]
        public ActionResult AddInterviewer(InterviewerViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user =
                    this.userViewFactory.Load(
                        new UserViewInputModel(UserName: model.Name, UserEmail: null));
                if (user == null)
                {
                    this.CommandService.Execute(
                        new CreateUserCommand(
                            publicKey: Guid.NewGuid(),
                            userName: model.Name,
                            password: SimpleHash.ComputeHash(model.Password),
                            email: model.Email,
                            isLocked: false,
                            roles: new[] { UserRoles.Operator },
                            supervsor: this.GetUser(model.Id).GetUseLight()));
                    return this.RedirectToAction("Interviewers", new { id = model.Id });
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
                var user =
                    this.userViewFactory.Load(
                        new UserViewInputModel(UserName: model.Name, UserEmail: null));
                if (user == null)
                {
                    this.CommandService.Execute(
                        new CreateUserCommand(
                            publicKey: Guid.NewGuid(),
                            userName: model.Name,
                            password: SimpleHash.ComputeHash(model.Password),
                            email: model.Email,
                            isLocked: false,
                            roles: new[] { UserRoles.Supervisor },
                            supervsor: null));

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
    }
}