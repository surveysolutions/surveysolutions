// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserController.cs" company="The World bank">
//   2012
// </copyright>
// <summary>
//  Define User controller
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Core.Supervisor.Views.Interviewer;
using Main.Core.View;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Core.Supervisor.Views.Summary;

    using Main.Core.Commands.User;
    using Main.Core.Entities.SubEntities;
    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;
    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Models;


    /// <summary>
    /// User controller responsible for dispay users, lock/unlock users, counting statistics
    /// </summary>
    [Authorize]
    public class UserController : Controller
    {
        #region Fields

        /// <summary>
        /// Global info object
        /// </summary>
        private readonly IGlobalInfoProvider globalInfo;

        /// <summary>
        /// View repository
        /// </summary>
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        /// <param name="globalInfo">
        /// The global info.
        /// </param>
        public UserController(IViewRepository viewRepository, IGlobalInfoProvider globalInfo)
        {
            this.viewRepository = viewRepository;
            this.globalInfo = globalInfo;
        }

        #endregion

        #region PublicActions

        /// <summary>
        /// Unlock user
        /// </summary>
        /// <param name="id">
        /// Use public key
        /// </param>
        /// <returns>
        /// Redirects to index view if everything is ok
        /// </returns>
        public ActionResult UnlockUser(string id)
        {
            return this.SetUserLock(id, false);
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
        public ActionResult LockUser(string id)
        {
            return this.SetUserLock(id, true);
        }

        /// <summary>
        /// Interviewer summary view
        /// </summary>
        /// <returns>
        /// Interviewer summary view
        /// </returns>
        public ActionResult Summary()
        {
            var user = this.globalInfo.GetCurrentUser();
            var model = this.viewRepository.Load<SummaryInputModel, SummaryView>(new SummaryInputModel(user));
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
        public ActionResult _SummaryData(GridDataRequestModel data)
        {
            var user = this.globalInfo.GetCurrentUser();
            var input = new SummaryInputModel(user)
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder
            };
            var model = this.viewRepository.Load<SummaryInputModel, SummaryView>(input);
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
            var model = this.viewRepository.Load<InterviewerStatisticsInputModel, InterviewerStatisticsView>(inputModel);
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
        public ActionResult Index(InterviewersInputModel input)
        {
            var user = this.globalInfo.GetCurrentUser();
            input.Supervisor = user;
            var model = this.viewRepository.Load<InterviewersInputModel, InterviewersView>(input);
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
        public ActionResult _TableData(GridDataRequestModel data)
        {
            var input = new InterviewersInputModel
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                Supervisor = new UserLight(data.SupervisorId, data.SupervisorName)
            };
            var model = this.viewRepository.Load<InterviewersInputModel, InterviewersView>(input);
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
            var model = this.viewRepository.Load<InterviewerInputModel, InterviewerView>(input);
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
        public ActionResult UserStatistics(GridDataRequestModel data)
        {
            var input = new InterviewerStatisticsInputModel()
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                UserId = data.UserId
            };
            var model = this.viewRepository.Load<InterviewerStatisticsInputModel, InterviewerStatisticsView>(input);
            return this.PartialView("_UserStatistics", model);
        }

        /// <summary>
        /// Uses to filter grids by user
        /// </summary>
        /// <returns>
        /// List of all  supervisor's users
        /// </returns>
        public ActionResult UsersJson()
        {
            var user = this.globalInfo.GetCurrentUser();
            var input = new InterviewersInputModel { PageSize = int.MaxValue, Supervisor = user };
            var model = this.viewRepository.Load<InterviewersInputModel, InterviewersView>(input);
            return this.Json(model.Items.ToDictionary(item => item.Id.ToString(), item => item.Login), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Private

        /// <summary>
        /// Change user lock status
        /// </summary>
        /// <param name="id">
        /// User public key
        /// </param>
        /// <param name="status">
        /// Lock status
        /// </param>
        /// <returns>
        /// Redirects to index view if everything is ok
        /// </returns>
        /// <exception cref="HttpException">
        /// Throws 404 exception if can not parse string guid
        /// </exception>
        private ActionResult SetUserLock(string id, bool status)
        {
            Guid key;
            if (!Guid.TryParse(id, out key))
                throw new HttpException("404");
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new ChangeUserStatusCommand(key, status));
            return this.RedirectToAction("Index");
        }

        #endregion
    }
}