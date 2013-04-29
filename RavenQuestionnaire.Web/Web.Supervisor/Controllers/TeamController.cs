// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TeamController.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
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

    /// <summary>
    ///     The teams controller.
    /// </summary>
    [Authorize]
    public class TeamController : BaseController
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamController"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="commandService">
        /// The command Service.
        /// </param>
        /// <param name="globalInfo">
        /// The global Info.
        /// </param>
        public TeamController(
            IViewRepository repository, ICommandService commandService, IGlobalInfoProvider globalInfo)
            : base(repository, commandService, globalInfo)
        {
            this.ViewBag.ActivePage = MenuItem.Teams;
        }

        #endregion

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
                this.CommandService.Execute(
                    new CreateUserCommand(
                        publicKey: Guid.NewGuid(), 
                        userName: model.Name, 
                        password: model.Password, 
                        email: model.Email, 
                        isLocked: false, 
                        roles: new[] { UserRoles.Operator }, 
                        supervsor: this.GetUser(model.Id).GetUseLight()));
                return this.RedirectToAction("Interviewers", new { id = model.Id });
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
                this.CommandService.Execute(
                    new CreateUserCommand(
                        publicKey: Guid.NewGuid(),
                        userName: model.Name,
                        password: model.Password,
                        email: model.Email,
                        isLocked: false,
                        roles: new[] { UserRoles.Supervisor },
                        supervsor: null));

                return this.RedirectToAction("Index");
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
        public ActionResult Data(GridDataRequestModel data)
        {
            UserListView model =
                this.Repository.Load<UserListViewInputModel, UserListView>(
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
        ///     The index.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" />.
        /// </returns>
        public ActionResult Index()
        {
            UserListView model =
                this.Repository.Load<UserListViewInputModel, UserListView>(
                    new UserListViewInputModel { Role = UserRoles.Supervisor });
            return this.View(model);
        }

        /// <summary>
        /// The interviewers.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The<see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Interviewers(Guid id)
        {
            UserView user = this.GetUser(id);
            InterviewersView interviewers =
                this.Repository.Load<InterviewersInputModel, InterviewersView>(
                    new InterviewersInputModel { SupervisorId = id });
            return this.View(new InterviewerListViewModel { View = interviewers, SupervisorName = user.UserName });
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
            return this.Repository.Load<UserViewInputModel, UserView>(new UserViewInputModel(id));
        }

        #endregion
    }
}