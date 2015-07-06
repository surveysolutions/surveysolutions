using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Resources;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class PeriodicStatusReportController : BaseController
    {
        private readonly IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory;

        public PeriodicStatusReportController(
            ICommandService commandService, 
            IGlobalInfoProvider globalInfo, 
            ILogger logger, 
            IViewFactory<AllUsersAndQuestionnairesInputModel, 
            AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory
            ) : base(commandService, globalInfo, logger)
        {
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
        }

        public ActionResult QuantityByInterviewers(Guid? supervisorId)
        {
            this.ViewBag.ActivePage = MenuItem.NumberOfCompletedInterviews;

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            var model = new PeriodicStatusReportModel
            {
                WebApiActionName = PeriodicStatusReportWebApiActionName.ByInterviewers,
                CanNavigateToQuantityByTeamMember = false,
                CanNavigateToQuantityBySupervisors = this.GlobalInfo.IsAdministrator || this.GlobalInfo.IsHeadquarter,
                Questionnaires = usersAndQuestionnaires.Questionnaires.ToArray(),
                ReportName = "Quantity",
                ResponsibleColumnName = PeriodicStatusReport.TeamMember,
                SupervisorId = supervisorId,
                ReportTypes = new[] {PeriodiceReportType.NumberOfCompletedInterviews}
            };

            return this.View("PeriodicStatusReport", model);
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult QuantityBySupervisors(PeriodiceReportType? reportType)
        {
            this.ViewBag.ActivePage = MenuItem.NumberOfCompletedInterviews;

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            var model = new PeriodicStatusReportModel
            {
                WebApiActionName = PeriodicStatusReportWebApiActionName.BySupervisors,
                CanNavigateToQuantityByTeamMember =
                    !reportType.HasValue || reportType == PeriodiceReportType.NumberOfCompletedInterviews,
                CanNavigateToQuantityBySupervisors = false,
                Questionnaires = usersAndQuestionnaires.Questionnaires.ToArray(),
                ReportName = "Quantity",
                ResponsibleColumnName = PeriodicStatusReport.Team,
                ReportTypes =
                    new[]
                    {
                        PeriodiceReportType.NumberOfCompletedInterviews,
                        PeriodiceReportType.NumberOfInterviewTransactionsByHQ,
                        PeriodiceReportType.NumberOfInterviewsApprovedByHQ,
                        PeriodiceReportType.NumberOfInterviewTransactionsBySupervisor
                    }
            };

            return this.View("PeriodicStatusReport", model);
        }
        public ActionResult SpeedByInterviewers(Guid? supervisorId)
        {
            this.ViewBag.ActivePage = MenuItem.SpeedOfCompletingInterviews;

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            var model = new PeriodicStatusReportModel
            {
                WebApiActionName = PeriodicStatusReportWebApiActionName.ByInterviewers,
                CanNavigateToQuantityByTeamMember = false,
                CanNavigateToQuantityBySupervisors = this.GlobalInfo.IsAdministrator || this.GlobalInfo.IsHeadquarter,
                Questionnaires = usersAndQuestionnaires.Questionnaires.ToArray(),
                ReportName = "Speed",
                ResponsibleColumnName = PeriodicStatusReport.TeamMember,
                SupervisorId = supervisorId,
                ReportTypes = new[] { PeriodiceReportType.AverageCaseAssignmentDuration}
            };

            return this.View("PeriodicStatusReport", model);
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult SpeedBySupervisors()
        {
            this.ViewBag.ActivePage = MenuItem.SpeedOfCompletingInterviews;

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            var model = new PeriodicStatusReportModel
            {
                WebApiActionName = PeriodicStatusReportWebApiActionName.BySupervisors,
                CanNavigateToQuantityByTeamMember = true,
                CanNavigateToQuantityBySupervisors = false,
                Questionnaires = usersAndQuestionnaires.Questionnaires.ToArray(),
                ReportName = "Speed",
                ResponsibleColumnName = PeriodicStatusReport.Team,
                ReportTypes = new[] { PeriodiceReportType.AverageCaseAssignmentDuration }
            };

            return this.View("PeriodicStatusReport", model);
        }
    }
}
