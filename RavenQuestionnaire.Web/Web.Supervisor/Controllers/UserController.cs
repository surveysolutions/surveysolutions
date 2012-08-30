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
    [Authorize]
    public class UserController : Controller
    {
        private readonly IGlobalInfoProvider globalInfo;
        private readonly IViewRepository viewRepository;

        public UserController(IViewRepository viewRepository, IGlobalInfoProvider globalInfo)
        {
            this.viewRepository = viewRepository;
            this.globalInfo = globalInfo;
        }

        //
        // GET: /User/
        public ActionResult UnlockUser(String id)
        {
            return SetUserLock(id, false);
        }

        public ActionResult LockUser(String id)
        {
            return SetUserLock(id, true);
        }

        private ActionResult SetUserLock(string id, bool status)
        {
            Guid key;
            if (!Guid.TryParse(id, out key))
                throw new HttpException("404");
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new ChangeUserStatusCommand(key, status));

            return RedirectToAction("Index");
        }

        public ActionResult Details(Guid id, InterviewerInputModel input)
        {
            var inputModel = input==null ? new InterviewerInputModel(){UserId = id} : new InterviewerInputModel()
                                                                               {
                                                                                   Order=input.Order, 
                                                                                   Orders = input.Orders, 
                                                                                   PageSize = input.PageSize, 
                                                                                   Page = input.Page, 
                                                                                   UserId=id, 
                                                                                   TemplateId = input.TemplateId
                                                                               };
            InterviewerView model = viewRepository.Load<InterviewerInputModel, InterviewerView>(inputModel);
            return View(model);
        }

        public ActionResult Index(InterviewersInputModel input)
        {
            UserLight user = globalInfo.GetCurrentUser();
            input.Supervisor = user;
            InterviewersView model = viewRepository.Load<InterviewersInputModel, InterviewersView>(input);
            return View(model);
        }

        public ActionResult _TableData(GridDataRequestModel data)
        {
            var input = new InterviewersInputModel
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                Supervisor = new UserLight(data.SupervisorId, data.SupervisorName)
            };
            InterviewersView model = viewRepository.Load<InterviewersInputModel, InterviewersView>(input);
            return PartialView("_Table", model);
        }

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
            InterviewerView model = viewRepository.Load<InterviewerInputModel, InterviewerView>(input);
            return PartialView("_TableGroupByUser", model.Items[0]);
        }
    }
}