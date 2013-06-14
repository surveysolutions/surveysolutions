using Main.Core.Utility;

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

    using WB.Core.SharedKernel.Logger;

    using Web.Supervisor.Models;

    [Authorize(Roles = "Headquarter")]
    public class TeamController : BaseController
    {
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;
        private readonly IViewFactory<UserListViewInputModel, UserListView> userListViewFactory;
        private readonly IViewFactory<InterviewersInputModel, InterviewersView> interviewersViewFactory;

        public TeamController(
            IViewRepository repository, ICommandService commandService, IGlobalInfoProvider globalInfo, ILog logger,
            IViewFactory<UserViewInputModel, UserView> userViewFactory, IViewFactory<UserListViewInputModel, UserListView> userListViewFactory, IViewFactory<InterviewersInputModel, InterviewersView> interviewersViewFactory)
            : base(repository, commandService, globalInfo, logger)
        {
            this.userViewFactory = userViewFactory;
            this.userListViewFactory = userListViewFactory;
            this.interviewersViewFactory = interviewersViewFactory;
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

        /// <summary>
        /// Gets table data for some view
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// Partial view with table's body
        /// </returns>
        public ActionResult GetSupervisors(GridDataRequestModel data)
        {
            var model =
                this.userListViewFactory.Load(
                    new UserListViewInputModel
                        {
                            Role = UserRoles.Supervisor, 
                            Orders = data.SortOrder, 
                            Page = data.Pager.Page, 
                            PageSize = data.Pager.PageSize
                        });
            return this.PartialView("_PartialGrid_Supervisors", model);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index(UserListViewInputModel data)
        {
            var model =
                this.userListViewFactory.Load(
                    new UserListViewInputModel
                        {
                            Role = UserRoles.Supervisor,
                            Page = data.Page,
                            PageSize = data.PageSize,
                            Orders = data.Orders
                        });
            return this.View(model);
        }

        /// <summary>
        /// The interviewers.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The<see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Interviewers(InterviewersViewInputModel data)
        {
            UserView user = this.GetUser(data.Id);
            var interviewers =
                this.interviewersViewFactory.Load(
                    new InterviewersInputModel
                        {
                            ViewerId = data.Id,
                            Page = data.Page,
                            PageSize = data.PageSize,
                            Order = data.Order
                        });
            return
                this.View(
                    new InterviewerListViewModel
                        {
                            View = interviewers,
                            Id = user.PublicKey,
                            SupervisorName = user.UserName
                        });
        }

        /// <summary>
        /// Gets table data for some view
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// Partial view with table's body
        /// </returns>
        public ActionResult GetInterviewers(InterviewersViewInputModel data)
        {
            UserView user = this.GetUser(data.Id);
            var interviewers =
                this.interviewersViewFactory.Load(
                    new InterviewersInputModel
                    {
                        ViewerId = data.Id,
                        Page = data.Page,
                        PageSize = data.PageSize,
                        Order = data.Order
                    });
            return this.PartialView("_PartialGridInterviewers", interviewers);
        }

        /// <summary>
        /// Unlock user
        /// </summary>
        /// <param name="id">
        /// Use public key
        /// </param>
        /// <returns>
        /// Redirects to index view if everything is ok
        /// </returns>
        [Authorize]
        public ActionResult UnlockUser(Guid id)
        {
            CommandService.Execute(new UnlockUserCommand(id));

            return this.Redirect(GlobalHelper.PreviousPage);
        }

        /// <summary>
        /// Lock user
        /// </summary>
        /// <param name="id">
        /// Use public key
        /// </param>
        /// <returns>
        /// Redirects to index view if everything is ok
        /// </returns>
        [Authorize]
        public ActionResult LockUser(Guid id)
        {
            CommandService.Execute(new LockUserCommand(id));

            return this.Redirect(GlobalHelper.PreviousPage);
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