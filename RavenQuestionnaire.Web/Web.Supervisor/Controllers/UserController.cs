using Ncqrs;
using System;
using System.Web;
using System.Web.Mvc;
using Web.Supervisor.Models;
using RavenQuestionnaire.Core;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core.Views.User;
using RavenQuestionnaire.Core.Commands.User;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace Web.Supervisor.Controllers
{
    using RavenQuestionnaire.Core.Views.Interviewer;

    /// <summary>
    /// User controller
    /// </summary>
    [Authorize]
    public class UserController : Controller
    {
        /// <summary>
        /// Global info object
        /// </summary>
        private readonly IGlobalInfoProvider globalInfo;

        /// <summary>
        /// View repository
        /// </summary>
        private readonly IViewRepository viewRepository;

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
        /// Display user's statistics
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// </returns>
        public ActionResult Details(Guid id, InterviewerInputModel input)
        {
            var inputModel = input == null
                                 ? new InterviewerInputModel() { UserId = id }
                                 : new InterviewerInputModel()
                                     {
                                         Order = input.Order,
                                         Orders = input.Orders,
                                         PageSize = input.PageSize,
                                         Page = input.Page,
                                         UserId = id,
                                         TemplateId = input.TemplateId
                                     };
            InterviewerView model = this.viewRepository.Load<InterviewerInputModel, InterviewerView>(inputModel);
            return this.View(model);
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
                                     UserId = id
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
            UserLight user = this.globalInfo.GetCurrentUser();
            input.Supervisor = user;
            InterviewersView model = this.viewRepository.Load<InterviewersInputModel, InterviewersView>(input);
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
            InterviewersView model = this.viewRepository.Load<InterviewersInputModel, InterviewersView>(input);
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
            {
                throw new HttpException("404");
            }

            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new ChangeUserStatusCommand(key, status));

            return this.RedirectToAction("Index");
        }
    }
}