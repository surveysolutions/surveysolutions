using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Status;
using RavenQuestionnaire.Core.Views.Status.Browse;
using RavenQuestionnaire.Core.Views.Status.StatusElement;
using RavenQuestionnaire.Core.Views.Status.SubView;
using RavenQuestionnaire.Core.Views.StatusReport;

namespace RavenQuestionnaire.Web.Controllers
{
    [QuestionnaireAuthorize(UserRoles.Administrator)]
    public class StatusController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;

        public StatusController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }

        public ViewResult Index()
        {
            var model = viewRepository.Load<StatusReportViewInputModel, StatusReportView>(new StatusReportViewInputModel());
            return View(model);
        }

        public ViewResult Details(string Qid)
        {
            if ( string.IsNullOrEmpty(Qid))
                throw new HttpException(404, "Invalid query string parameters.");

            var model = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(Qid));
            return View(model);
        }

        public ViewResult Create(string qId, string Id)
        {
            return View(new StatusItemView()
                            {
                                QuestionnaireId = qId,
                                StatusId = Id
                            }
        );
        }

        [HttpPost]
        public ActionResult Save(StatusItemView model)
        {
            if (ModelState.IsValid && model != null && !string.IsNullOrEmpty(model.StatusId))
            {
                if (model.PublicKey == null || model.PublicKey == Guid.Empty)
                {
                    commandInvoker.Execute(new CreateNewStatusCommand(model.Title, model.IsInitial, model.StatusId, model.QuestionnaireId, GlobalInfo.GetCurrentUser()));
                }
                return RedirectToAction("Details", new
                {
                    Qid = model.QuestionnaireId
                });
            }
            return View("Create", model);
        }

        [HttpPost]
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Update(StatusItemView model)
        {
            if (ModelState.IsValid)
            {
                if (model.PublicKey != null && model.StatusRolesMatrix != null)
                {
                    Dictionary<string, List<SurveyStatus>> roles = new Dictionary<string, List<SurveyStatus>>();

                    foreach (var item in model.StatusRolesMatrix)
                    {
                        foreach (var roleItem in item.StatusRestriction)
                            if (roleItem.Permit)
                            {
                                if (!roles.ContainsKey(roleItem.RoleName))
                                    roles.Add(roleItem.RoleName, new List<SurveyStatus>());
                                roles[roleItem.RoleName].Add(new SurveyStatus(item.Status.PublicKey, item.Status.Title));
                            }
                    }

                    commandInvoker.Execute(new UpdateStatusRestrictionsCommand(model.QuestionnaireId, model.StatusId, model.PublicKey, roles, GlobalInfo.GetCurrentUser()));
                    return RedirectToAction("Details", new
                    {
                        Qid = model.QuestionnaireId
                    });
                }
            }

            AddRolesListToViewBag();
            return View("Edit", model);
            
        }

        protected void AddStatusListToViewBag(string Qid)
        {
            var statuses = viewRepository.Load<StatusBrowseInputModel, StatusBrowseView>
                (new StatusBrowseInputModel()
                     {
                         PageSize = 300, 
                         QId = Qid
                     }).Items;

            ViewBag.AllStatuses = statuses;
        }

        protected void AddRolesListToViewBag()
        {
            var roles = Roles.GetAllRoles();
            ViewBag.AllRoles = roles;
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ViewResult Edit(string Qid, Guid PublicKey)
        {
            if (string.IsNullOrEmpty(Qid) || PublicKey == null )
                throw new HttpException(404, "Invalid query string parameters.");

            StatusView model = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(Qid));

            //rewrite!!!
            if (model != null)
            {
                var currentStatus = model.StatusElements.FirstOrDefault(x => x.PublicKey == PublicKey);

                foreach (var status in model.StatusElements)
                {
                    var statusByRole = new StatusByRole {Status = status};

                    foreach (var role in Roles.GetAllRoles())
                    {
                        bool flag = false;
                        if (currentStatus.StatusRoles.ContainsKey(role))
                        {
                            foreach (var item in currentStatus.StatusRoles[role])
                            {
                                if (item.PublicId == status.PublicKey)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        statusByRole.StatusRestriction.Add(new RolePermission(role, flag));
                    }
                    currentStatus.StatusRolesMatrix.Add(statusByRole);
                }

                AddRolesListToViewBag();

                return View("Edit", currentStatus);
            }

            AddRolesListToViewBag();
            return View("Edit", null);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ViewResult Route(string Qid, Guid publicKey)
        {
            if (string.IsNullOrEmpty(Qid) || publicKey == null)
                throw new HttpException(404, "Invalid query string parameters.");

            StatusView model = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(Qid));

            StatusItemView item = null;
            if (model != null)
                item = model.StatusElements.FirstOrDefault(x => x.PublicKey == publicKey);
            return View(item);
        }

        [HttpPost]
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult SaveRoute(string StatusId, SurveyStatus TargetStatus, string changeComment, Guid PublicKey,  string ConditionExpression)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(StatusId))
                {
                    commandInvoker.Execute(new AddNewStatusFlowItem(StatusId, ConditionExpression,
                        changeComment, TargetStatus, PublicKey, GlobalInfo.GetCurrentUser()));
                }

                StatusView m = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(StatusId));
                return PartialView("_Route", m);
            }
            return View("AddRoute");
        }

       
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult EditRoute(Guid publicId, string Qid, string statusId)
        {
            AddStatusListToViewBag(Qid);

            StatusView model = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(Qid));

            if (model != null)
            {
                var currentStatus = model.StatusElements.FirstOrDefault(x => x.PublicKey == publicId);

                if (currentStatus.FlowRules.ContainsKey(publicId))
                    return PartialView("AddRoute", currentStatus.FlowRules[publicId]);

            }

            throw new HttpException(404, "Invalid query string parameters.");
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult AddRoute(string id, string qid)
        {
            AddStatusListToViewBag(qid);
            return PartialView("AddRoute", new FlowRule() { StatusId = id });            
        }
    }
}
