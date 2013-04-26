// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TeamsController.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web.Mvc;

    using Core.Supervisor.Views.Interviewer;
    using Core.Supervisor.Views.User;

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
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Interviewers(string id)
        {
            Guid supervisorId = this.ParseKeyOrThrow404(id);

            InterviewersView model =
                this.Repository.Load<InterviewersInputModel, InterviewersView>(
                    new InterviewersInputModel() { SupervisorId = supervisorId });
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

        #endregion
    }
}