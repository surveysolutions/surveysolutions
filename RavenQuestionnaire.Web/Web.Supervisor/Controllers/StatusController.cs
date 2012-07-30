using System;
using System.Collections.Generic;

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
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Views.User;

namespace Web.Supervisor.Controllers
{

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

        public ViewResult ItemList(QuestionnaireBrowseInputModel input)
        {
            var model = viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input);
            return View(model);
        }
        public ViewResult UserList(UserBrowseInputModel input)
        {
            var model = viewRepository.Load<UserBrowseInputModel, UserBrowseView>(input);
            return View(model);
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ViewResult Create(string qId, string Id)
        {
            return View(new StatusItemView()
            {
                QuestionnaireId = qId,
                StatusId = Id
            }
        );
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





    }
}
