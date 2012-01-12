using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Status;
using RavenQuestionnaire.Core.Views.Status.SubView;

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

        public ViewResult Index(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters.");

            StatusBrowseInputModel input = new StatusBrowseInputModel {QuestionnaireId = id};
            var model = viewRepository.Load<StatusBrowseInputModel, StatusBrowseView>(input);
            return View(model);
        }

        public ViewResult Create(string qId)
        {
            return View(StatusBrowseItem.New(qId));
        }

        [HttpPost]
        public ActionResult Save(StatusBrowseItem model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Id))
                {
                    commandInvoker.Execute(new CreateNewStatusCommand(model.Title, model.IsInitial, model.QuestionnaireId, GlobalInfo.GetCurrentUser()));
                }
                return RedirectToAction("Index", new
                {
                    id = model.QuestionnaireId
                });

            }
            return View("Create", model);
        }

        [HttpPost]
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Update(StatusView model)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(model.Id) && model.StatusRolesMatrix != null)
                {
                    Dictionary<string, List<SurveyStatus>> roles = new Dictionary<string, List<SurveyStatus>>();
                    
                    foreach (var item in model.StatusRolesMatrix)
                    {
                        foreach (var roleItem in item.StatusRestriction)
                            if (roleItem.Permit)
                            {
                                if (!roles.ContainsKey(roleItem.RoleName))
                                    roles.Add(roleItem.RoleName, new List<SurveyStatus>());
                                roles[roleItem.RoleName].Add(new SurveyStatus(item.Status.Id, item.Status.Title));
                            }
                    }
                    commandInvoker.Execute(new UpdateStatusRestrictionsCommand(model.Id, roles, GlobalInfo.GetCurrentUser()));
                    return RedirectToAction("Index", new
                    {
                        id = model.QuestionnaireId

                    });
                }
            }

            AddRolesListToViewBag();
            return View("Edit", model);
            
        }

        protected void AddStatusListToViewBag()
        {
            var statuses =
              viewRepository.Load<StatusBrowseInputModel, StatusBrowseView>(new StatusBrowseInputModel() { PageSize = 100 }).Items;

            ViewBag.AllStatuses = statuses;
        }

        protected void AddRolesListToViewBag()
        {
            var roles = Roles.GetAllRoles();
            ViewBag.AllRoles = roles;
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ViewResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters.");
            StatusView model = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(id));

            if (model != null)
            {
                foreach (var status in viewRepository.Load<StatusBrowseInputModel, StatusBrowseView>(
                    new StatusBrowseInputModel() { PageSize = 100, QuestionnaireId = model.QuestionnaireId}).Items)
                {
                    var statusByRole = new StatusByRole {Status = status};

                    foreach (var role in Roles.GetAllRoles())
                    {
                        bool flag = false;
                        if (model.StatusRoles.ContainsKey(role))
                        {
                            foreach (var item in model.StatusRoles[role])
                            {
                                if (String.Compare(item.Id, status.Id, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        
                        statusByRole.StatusRestriction.Add(new RolePermission(role, flag));
                    }

                    model.StatusRolesMatrix.Add(statusByRole);
                }
            }
            AddRolesListToViewBag();
            return View("Edit", model);
        }

    }
}
