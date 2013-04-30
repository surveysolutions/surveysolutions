// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserController.cs" company="The World bank">
//   2012
// </copyright>
// <summary>
//  Define User controller
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Web.Http;
using System.Web.Security;
using Core.Supervisor.Views.Interviewer;
using Main.Core.Utility;
using Main.Core.View;
using Questionnaire.Core.Web.Security;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Core.Supervisor.Views.Summary;

    using Main.Core.Commands.User;
    using Main.Core.Entities.SubEntities;
    using Ncqrs.Commanding.ServiceModel;
    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Models;
    using Web.Supervisor.Models.Chart;

    /// <summary>
    /// User controller responsible for dispay users, lock/unlock users, counting statistics
    /// </summary>
   
    public class UserController : BaseController
    {
        private readonly IFormsAuthentication authentication;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="auth">
        /// The auth.
        /// </param>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        /// <param name="commandService">
        /// The command service.
        /// </param>
        /// <param name="globalInfo">
        /// The global info.
        /// </param>
        public UserController(
            IFormsAuthentication auth,
            IViewRepository viewRepository,
            ICommandService commandService,
            IGlobalInfoProvider globalInfo)
            : base(viewRepository, commandService, globalInfo)
        {
            this.authentication = auth;
        }

        [AllowAnonymous]
        public ActionResult CreateSupervisor()
        {
            var supervisor = new SupervisorModel(Guid.NewGuid(), "supervisor") {Role = UserRoles.Headquarter};
            
            return this.View(supervisor);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CreateSupervisor(SupervisorModel user)
        {
            if (ModelState.IsValid)
            {
                CommandService.Execute(new CreateUserCommand(user.Id, user.Name, SimpleHash.ComputeHash(user.Name),
                                                             user.Name + "@worldbank.org", new[] {user.Role},
                                                             false, null));

                
                var isSupervisor = Roles.IsUserInRole(user.Name, UserRoles.Supervisor.ToString());
                var isHeadquarter = Roles.IsUserInRole(user.Name, UserRoles.Headquarter.ToString());
                if (isSupervisor || isHeadquarter)
                {
                    this.authentication.SignIn(user.Name, false);
                    if (isSupervisor)
                        return this.Redirect("~/");
                    else
                        return this.RedirectToRoute("HeadquarterDashboard");
                }
            }

            return this.View(user);
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

        /// <summary>
        /// Interviewer summary view
        /// </summary>
        /// <returns>
        /// Interviewer summary view
        /// </returns>
         [Authorize]
        public ActionResult Summary()
        {
            ViewBag.ActivePage = MenuItem.Interviewers;
            var user = this.GlobalInfo.GetCurrentUser();
            var model = this.Repository.Load<SummaryInputModel, SummaryView>(new SummaryInputModel(user));
            ViewBag.GraphData = new SurveyChartModel(model);
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
         [Authorize]
        public ActionResult _SummaryData(GridDataRequestModel data)
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var input = new SummaryInputModel(user)
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                TemplateId = data.TemplateId
            };
            var model = this.Repository.Load<SummaryInputModel, SummaryView>(input);
            ViewBag.GraphData = new SurveyChartModel(model);
            return this.PartialView("_SummaryTable", model);
        }

        /// <summary>
        /// Display user's statistics grouped by surveys and statuses
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// Show statistics view if everything is ok
        /// </returns>
         [Authorize]
        public ActionResult Statistics(Guid id, InterviewerStatisticsInputModel input)
        {
            var inputModel = input == null
                ? new InterviewerStatisticsInputModel() { UserId = id }
                : new InterviewerStatisticsInputModel()
                                 {
                                     Order = input.Order,
                                     Orders = input.Orders,
                                     PageSize = input.PageSize,
                                     Page = input.Page,
                                     UserId = id,
                                     UserName = input.UserName
                                 };
            var model = this.Repository.Load<InterviewerStatisticsInputModel, InterviewerStatisticsView>(inputModel);
            return this.View(model);
        }

        /// <summary>
        /// User index page. Shows grid with supervisor's statistics grouped by interviewers
        /// </summary>
        /// <param name="input">
        /// The input model
        /// </param>
        /// <returns>
        /// Index view
        /// </returns>
         [Authorize]
        public ActionResult Index(InterviewersInputModel input)
        {
            ViewBag.ActivePage = MenuItem.Administration;
            var user = this.GlobalInfo.GetCurrentUser();
            input.SupervisorId = user.Id;
            var model = this.Repository.Load<InterviewersInputModel, InterviewersView>(input);
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
         [Authorize]
        public ActionResult _TableData(GridDataRequestModel data)
        {
            var input = new InterviewersInputModel
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                SupervisorId = data.SupervisorId
            };
            var model = this.Repository.Load<InterviewersInputModel, InterviewersView>(input);
            return this.PartialView("_Table", model);
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
        [HttpPost]
        [Authorize]
        public ActionResult TableGroupByUser(GridDataRequestModel data)
        {
            var input = new InterviewerInputModel()
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                TemplateId = data.TemplateId,
                UserId = data.UserId
            };
            var model = this.Repository.Load<InterviewerInputModel, InterviewerView>(input);
            return this.PartialView("_TableGroupByUser", model.Items[0]);
        }

        /// <summary>
        /// Gets user's statistics
        /// </summary>
        /// <param name="data">
        /// Table order data
        /// </param>
        /// <returns>
        /// Partial view with table's body
        /// </returns>
        [HttpPost]
        [Authorize]
        public ActionResult UserStatistics(GridDataRequestModel data)
        {
            var input = new InterviewerStatisticsInputModel()
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                UserId = data.UserId
            };
            var model = this.Repository.Load<InterviewerStatisticsInputModel, InterviewerStatisticsView>(input);
            return this.PartialView("_UserStatistics", model);
        }

        /// <summary>
        /// Uses to filter grids by user
        /// </summary>
        /// <returns>
        /// List of all  supervisor's users
        /// </returns>
         [Authorize]
        public ActionResult UsersJson()
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var input = new InterviewersInputModel { PageSize = int.MaxValue, SupervisorId = user.Id };
            var model = this.Repository.Load<InterviewersInputModel, InterviewersView>(input);
            return this.Json(model.Items.ToDictionary(item => item.Id.ToString(), item => item.Login), JsonRequestBehavior.AllowGet);
        }
    }
}