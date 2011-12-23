using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
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

        public ActionResult Index(StatusBrowseInputModel input)
        {
            var model = viewRepository.Load<StatusBrowseInputModel, StatusBrowseView>(input);
            return View(model);
        }

        public ActionResult Create()
        {
            return View(StatusBrowseItem.New());
        }

        [HttpPost]
        public ActionResult Save(StatusBrowseItem model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Id))
                {
                    commandInvoker.Execute(new CreateNewStatusCommand(model.Title, model.IsInitial, Global.GetCurrentUser()));
                }
                return RedirectToAction("Index");

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
                    commandInvoker.Execute(new UpdateStatusRestrictionsCommand(model.Id, roles, Global.GetCurrentUser()));
                    return RedirectToAction("Index");
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
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters.");
            StatusView model = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(id));

            if (model != null)
            {
                foreach (var status in viewRepository.Load<StatusBrowseInputModel, StatusBrowseView>(
                    new StatusBrowseInputModel() { PageSize = 100 }).Items)
                {
                    var statusByRole = new StatusByRole {Status = status};

                    foreach (var role in Roles.GetAllRoles())
                    {
                        bool flag = false;
                        if (model.StatusRoles.ContainsKey(role))
                        {
                            foreach (var VARIABLE in model.StatusRoles[role])
                            {
                                if (String.Compare(VARIABLE.Id, status.Id, StringComparison.OrdinalIgnoreCase) == 0)
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
