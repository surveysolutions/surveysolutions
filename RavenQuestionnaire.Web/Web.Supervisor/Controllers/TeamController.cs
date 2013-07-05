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

        // GET: /Teams/
        #region Public Methods and Operators

        /// <summary>
        /// The add interviewer.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [Authorize(Roles = "Headquarter")]
        public ActionResult AddInterviewer(Guid id)
        {
            return this.View(new InterviewerViewModel { Id = id });
        }

        /// <summary>
        /// The add interviewer.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
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

        /// <summary>
        ///     The add supervisor.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" />.
        /// </returns>
        [Authorize(Roles = "Headquarter")]
        public ActionResult AddSupervisor()
        {
            return this.View(new SupervisorViewModel());
        }

        /// <summary>
        /// The add supervisor.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
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

        #endregion

        #region Methods

        /// <summary>
        /// The get user.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="UserView"/>.
        /// </returns>
        private UserView GetUser(Guid id)
        {
            return this.userViewFactory.Load(new UserViewInputModel(id));
        }

        #endregion
    }
}