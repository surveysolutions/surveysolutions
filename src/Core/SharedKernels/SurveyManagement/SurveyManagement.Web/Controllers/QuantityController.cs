using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class QuantityController : BaseController
    {
        private readonly IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory;

        public QuantityController(
            ICommandService commandService, 
            IGlobalInfoProvider globalInfo, 
            ILogger logger, 
            IViewFactory<AllUsersAndQuestionnairesInputModel, 
            AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory
            ) : base(commandService, globalInfo, logger)
        {
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
        }

        public ActionResult ByInterviewers(Guid? supervisorId)
        {
            this.ViewBag.ActivePage = MenuItem.NumberOfCompletedInterviews;
            this.ViewBag.WebApiActionName = "QuantityByInterviewers";
            this.ViewBag.CanNavigateToQuantityByTeamMember = false;
            this.ViewBag.CanNavigateToQuantityBySupervisors = this.GlobalInfo.IsAdministrator || this.GlobalInfo.IsHeadquarter;

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());
            return this.View("QuantityByResponsibles", usersAndQuestionnaires.Questionnaires);
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult BySupervisors()
        {
            this.ViewBag.ActivePage = MenuItem.NumberOfCompletedInterviews;
            this.ViewBag.WebApiActionName = "QuantityBySupervisors";
            this.ViewBag.CanNavigateToQuantityByTeamMember = true;
            this.ViewBag.CanNavigateToQuantityBySupervisors = false;
            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());
            return this.View("QuantityByResponsibles", usersAndQuestionnaires.Questionnaires);
        }
    }
}
