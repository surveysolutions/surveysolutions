using System;
using System.Linq;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Resources;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    public class PeriodicStatusReportController : BaseController
    {
        private readonly IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory;

        private readonly PeriodiceReportType[] reportTypesWhichShouldBeReroutedToCustomStatusController =
        {
            PeriodiceReportType.AverageOverallCaseProcessingTime, 
            PeriodiceReportType.AverageCaseAssignmentDuration
        };

        private readonly PeriodiceReportType[] speedReportTypes =
        {
            PeriodiceReportType.AverageInterviewDuration,
            PeriodiceReportType.AverageSupervisorProcessingTime,
            PeriodiceReportType.AverageHQProcessingTime,
            PeriodiceReportType.AverageCaseAssignmentDuration,
            PeriodiceReportType.AverageOverallCaseProcessingTime
        };

        private readonly PeriodiceReportType[] quantityReportTypes =
        {
            PeriodiceReportType.NumberOfCompletedInterviews,
            PeriodiceReportType.NumberOfInterviewTransactionsByHQ,
            PeriodiceReportType.NumberOfInterviewsApprovedByHQ,
            PeriodiceReportType.NumberOfInterviewTransactionsBySupervisor
        };

        public PeriodicStatusReportController(
            ICommandService commandService,
            IGlobalInfoProvider globalInfo,
            ILogger logger,
            IViewFactory<AllUsersAndQuestionnairesInputModel,
            AllUsersAndQuestionnairesView> allUsersAndQuestionnairesFactory
            )
            : base(commandService, globalInfo, logger)
        {
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
        }

        public ActionResult QuantityByInterviewers(Guid? supervisorId, PeriodiceReportType reportType = PeriodiceReportType.NumberOfCompletedInterviews)
        {
            this.ViewBag.ActivePage = MenuItem.NumberOfCompletedInterviews;

            var model = this.CreatePeriodicStatusReportModel(
                reportType: reportType,
                webApiActionName: PeriodicStatusReportWebApiActionName.ByInterviewers,
                canNavigateToQuantityByTeamMember: false,
                canNavigateToQuantityBySupervisors: this.GlobalInfo.IsAdministrator || this.GlobalInfo.IsHeadquarter,
                reportName: "Quantity",
                responsibleColumnName: PeriodicStatusReport.TeamMember,
                supervisorId: supervisorId);

            model.ReportTypes = quantityReportTypes;

            return this.View("PeriodicStatusReport", model);
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult QuantityBySupervisors(PeriodiceReportType reportType = PeriodiceReportType.NumberOfCompletedInterviews)
        {
            this.ViewBag.ActivePage = MenuItem.NumberOfCompletedInterviews;

            var model = this.CreatePeriodicStatusReportModel(
                reportType: reportType,
                webApiActionName: PeriodicStatusReportWebApiActionName.BySupervisors,
                canNavigateToQuantityByTeamMember: true,
                canNavigateToQuantityBySupervisors: false,
                reportName: "Quantity",
                responsibleColumnName: PeriodicStatusReport.Team);

            model.ReportTypes = quantityReportTypes;

            return this.View("PeriodicStatusReport", model);
        }

        public ActionResult SpeedByInterviewers(Guid? supervisorId, PeriodiceReportType reportType = PeriodiceReportType.NumberOfCompletedInterviews)
        {
            this.ViewBag.ActivePage = MenuItem.SpeedOfCompletingInterviews;

            var periodicStatusReportWebApiActionName =
             reportTypesWhichShouldBeReroutedToCustomStatusController.Contains(reportType)
                 ? PeriodicStatusReportWebApiActionName.BetweenStatusesByInterviewers
                 : PeriodicStatusReportWebApiActionName.ByInterviewers;

            var model = this.CreatePeriodicStatusReportModel(
                reportType: reportType,
                webApiActionName: periodicStatusReportWebApiActionName,
                canNavigateToQuantityByTeamMember: false,
                canNavigateToQuantityBySupervisors: this.GlobalInfo.IsAdministrator || this.GlobalInfo.IsHeadquarter,
                reportName: "Speed",
                responsibleColumnName: PeriodicStatusReport.TeamMember,
                supervisorId: supervisorId);

            model.ReportTypes = speedReportTypes;

            return this.View("PeriodicStatusReport", model);
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult SpeedBySupervisors(PeriodiceReportType reportType = PeriodiceReportType.AverageInterviewDuration)
        {
            this.ViewBag.ActivePage = MenuItem.SpeedOfCompletingInterviews;

            var periodicStatusReportWebApiActionName =
                reportTypesWhichShouldBeReroutedToCustomStatusController.Contains(reportType)
                    ? PeriodicStatusReportWebApiActionName.BetweenStatusesBySupervisors
                    : PeriodicStatusReportWebApiActionName.BySupervisors;

            var model = this.CreatePeriodicStatusReportModel(
                reportType: reportType,
                webApiActionName: periodicStatusReportWebApiActionName,
                canNavigateToQuantityByTeamMember: true,
                canNavigateToQuantityBySupervisors: false,
                reportName: "Speed",
                responsibleColumnName: PeriodicStatusReport.Team);

            model.ReportTypes = speedReportTypes;

            return this.View("PeriodicStatusReport", model);
        }

        private PeriodicStatusReportModel CreatePeriodicStatusReportModel(PeriodicStatusReportWebApiActionName webApiActionName, PeriodiceReportType reportType,
            bool canNavigateToQuantityByTeamMember, bool canNavigateToQuantityBySupervisors, string reportName, string responsibleColumnName, Guid? supervisorId = null)
        {
            var allUsersAndQuestionnaires = this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

            return new PeriodicStatusReportModel
            {
                WebApiActionName = webApiActionName,
                CanNavigateToQuantityByTeamMember = canNavigateToQuantityByTeamMember,
                CanNavigateToQuantityBySupervisors = canNavigateToQuantityBySupervisors,
                Questionnaires = allUsersAndQuestionnaires.Questionnaires.ToArray(),
                ReportName = reportName,
                ResponsibleColumnName = responsibleColumnName,
                SupervisorId = supervisorId,
                ReportNameDescription = string.Format(GetReportDescriptionByType(reportType), PeriodicStatusReport.Team.ToLower())
            };
        }

        private string GetReportDescriptionByType(PeriodiceReportType reportType)
        {
            switch (reportType)
            {
                case PeriodiceReportType.NumberOfCompletedInterviews:
                    return PeriodicStatusReport.NumberOfCompletedInterviewsDescription;
                case PeriodiceReportType.NumberOfInterviewTransactionsBySupervisor:
                    return PeriodicStatusReport.NumberOfInterviewTransactionsBySupervisorDescription;
                case PeriodiceReportType.NumberOfInterviewTransactionsByHQ:
                    return PeriodicStatusReport.NumberOfInterviewTransactionsByHQDescription;
                case PeriodiceReportType.NumberOfInterviewsApprovedByHQ:
                    return PeriodicStatusReport.NumberOfInterviewsApprovedByHQDescription;
                case PeriodiceReportType.AverageCaseAssignmentDuration:
                    return PeriodicStatusReport.AverageCaseAssignmentDurationDescription;
                case PeriodiceReportType.AverageInterviewDuration:
                    return PeriodicStatusReport.AverageInterviewDurationDescription;
                case PeriodiceReportType.AverageSupervisorProcessingTime:
                    return PeriodicStatusReport.AverageSupervisorProcessingTimeDescription;
                case PeriodiceReportType.AverageHQProcessingTime:
                    return PeriodicStatusReport.AverageHQProcessingTimeDescription;
                case PeriodiceReportType.AverageOverallCaseProcessingTime:
                    return PeriodicStatusReport.AverageOverallCaseProcessingTimeDescription;
            }

            return string.Empty;
        }
    }
}
