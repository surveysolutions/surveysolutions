using System;
using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    public class PeriodicStatusReportController : BaseController
    {
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;

        private readonly IUserViewFactory userViewFactory;

        private readonly PeriodiceReportType[] reportTypesWhichShouldBeReroutedToCustomStatusController =
        {
            PeriodiceReportType.AverageOverallCaseProcessingTime, 
            PeriodiceReportType.AverageCaseAssignmentDuration
        };

        private readonly PeriodiceReportType[] speedReportTypesForHeadquarters =
        {
            PeriodiceReportType.AverageInterviewDuration,
            PeriodiceReportType.AverageSupervisorProcessingTime,
            PeriodiceReportType.AverageHQProcessingTime,
            PeriodiceReportType.AverageCaseAssignmentDuration,
            PeriodiceReportType.AverageOverallCaseProcessingTime
        };

        private readonly PeriodiceReportType[] speedReportTypesForSupervisor =
        {
            PeriodiceReportType.AverageInterviewDuration,
            PeriodiceReportType.AverageCaseAssignmentDuration,
        };

        private readonly PeriodiceReportType[] quantityReportTypesForHeadquarters =
        {
            PeriodiceReportType.NumberOfCompletedInterviews,
            PeriodiceReportType.NumberOfInterviewTransactionsByHQ,
            PeriodiceReportType.NumberOfInterviewsApprovedByHQ,
            PeriodiceReportType.NumberOfInterviewTransactionsBySupervisor
        };

        private readonly PeriodiceReportType[] quantityReportTypesForSupervisor =
        {
            PeriodiceReportType.NumberOfCompletedInterviews,
            PeriodiceReportType.NumberOfInterviewTransactionsBySupervisor
        };

        public PeriodicStatusReportController(
            ICommandService commandService,
            IGlobalInfoProvider globalInfo,
            ILogger logger,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory, 
            IUserViewFactory userViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.userViewFactory = userViewFactory;
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
                totalRowPresent: true,
                supervisorId: supervisorId);

            model.ReportTypes = this.quantityReportTypesForSupervisor;
            model.SupervisorName = GetSupervisorName(supervisorId);

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
                totalRowPresent: true,
                responsibleColumnName: PeriodicStatusReport.Team);

            model.ReportTypes = this.quantityReportTypesForHeadquarters;

            return this.View("PeriodicStatusReport", model);
        }

        public ActionResult SpeedByInterviewers(Guid? supervisorId, PeriodiceReportType reportType = PeriodiceReportType.AverageInterviewDuration)
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
                totalRowPresent: false,
                supervisorId: supervisorId);

            model.ReportTypes = this.speedReportTypesForSupervisor;
            model.SupervisorName = GetSupervisorName(supervisorId);

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
                reportName: "Speed", totalRowPresent: false,
                responsibleColumnName: PeriodicStatusReport.Team);

            model.ReportTypes = this.speedReportTypesForHeadquarters;

            return this.View("PeriodicStatusReport", model);
        }

        private PeriodicStatusReportModel CreatePeriodicStatusReportModel(PeriodicStatusReportWebApiActionName webApiActionName, PeriodiceReportType reportType,
            bool canNavigateToQuantityByTeamMember, bool canNavigateToQuantityBySupervisors, string reportName, string responsibleColumnName, bool totalRowPresent, Guid? supervisorId = null)
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
                ReportNameDescription = string.Format(GetReportDescriptionByType(reportType), PeriodicStatusReport.Team.ToLower()),
                TotalRowPresent = totalRowPresent
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

        private string GetSupervisorName(Guid? supervisorId)
        {
            if (!supervisorId.HasValue)
                return null;

            var userView =this.userViewFactory.Load(new UserViewInputModel(supervisorId.Value));
            return userView?.UserName;
        }
    }
}
