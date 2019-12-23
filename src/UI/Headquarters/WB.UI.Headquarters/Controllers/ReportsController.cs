using System;
using System.Linq;
using System.Web.Mvc;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Reports;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    public class ReportsController : Controller
    {
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUserViewFactory userViewFactory;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses;
        private readonly IMapReport mapReport;

        public ReportsController(IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory,
            IAuthorizedUser authorizedUser,
            IUserViewFactory userViewFactory,
            IChartStatisticsViewFactory chartStatisticsViewFactory,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses, 
            IMapReport mapReport)
        {
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.authorizedUser = authorizedUser;
            this.userViewFactory = userViewFactory;
            this.chartStatisticsViewFactory = chartStatisticsViewFactory;
            this.interviewStatuses = interviewStatuses;
            this.mapReport = mapReport;
        }

        [AuthorizeOr403(Roles = "Supervisor")]
        [ActivePage(MenuItem.SurveyAndStatuses)]
        public ActionResult SurveysAndStatusesForSv(SurveysAndStatusesModel model)
        {
            return this.View(model);
        }


        [AuthorizeOr403(Roles = "Administrator, Supervisor, Headquarter")]
        [ActivePage(MenuItem.MapReport)]
        public ActionResult MapReport()
        {
            var questionnaires = this.mapReport.GetQuestionnaireIdentitiesWithGpsQuestions();

            return View(new
            {
                Questionnaires = questionnaires.GetQuestionnaireComboboxViewItems()
            });
        }


        [ActivePage(MenuItem.StatusDuration)]
        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
        public ActionResult StatusDuration()
        {
            return this.View("StatusDuration", new StatusDurationModel
            {
                DataUrl = Url.RouteUrl("DefaultApiWithAction",
                    new
                    {
                        httproute = "",
                        controller = "ReportDataApi",
                        action = "StatusDuration"
                    }),
                InterviewsBaseUrl = Url.Action("Index", "Interviews"),
                AssignmentsBaseUrl = Url.Action("Index", "Assignments"),
                QuestionnairesUrl = Url.RouteUrl("DefaultApiWithAction",
                    new {httproute = "", controller = "QuestionnairesApi", action = "QuestionnairesWithVersions"}),
                QuestionnaireByIdUrl = Url.RouteUrl("DefaultApiWithAction",
                    new { httproute = "", controller = "QuestionnairesApi", action = "QuestionnairesComboboxById" })
            });
        }

        [ActivePage(MenuItem.StatusDuration)]
        [AuthorizeOr403(Roles = "Supervisor")]
        public ActionResult TeamStatusDuration()
        {
            return this.View("StatusDuration", new StatusDurationModel
            {
                IsSupervisorMode = true,

                DataUrl = Url.RouteUrl("DefaultApiWithAction",
                    new
                    {
                        httproute = "",
                        controller = "ReportDataApi",
                        action = "TeamStatusDuration"
                    }),

                InterviewsBaseUrl = Url.Action("Index", "Interviews"),
                AssignmentsBaseUrl = Url.Action("Index", "Assignments"),
                QuestionnairesUrl = Url.RouteUrl("DefaultApiWithAction",
                    new { httproute = "", controller = "QuestionnairesApi", action = "QuestionnairesWithVersions" }),
                QuestionnaireByIdUrl = Url.RouteUrl("DefaultApiWithAction",
                    new { httproute = "", controller = "QuestionnairesApi", action = "QuestionnairesComboboxById" })
            });
        }

        [ActivePage(MenuItem.SurveyStatistics)]
        [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult SurveyStatistics()
        {
            return this.View("SurveyStatistics", new
            {
                isSupervisor = this.authorizedUser.IsSupervisor
            });
        }
        
        public ActionResult QuantityByInterviewers(Guid? supervisorId, PeriodiceReportType reportType = PeriodiceReportType.NumberOfCompletedInterviews)
        {
            this.ViewBag.ActivePage = MenuItem.NumberOfCompletedInterviews;

            var model = this.CreatePeriodicStatusReportModel(
                reportType: reportType,
                webApiActionName: PeriodicStatusReportWebApiActionName.ByInterviewers,
                canNavigateToQuantityByTeamMember: false,
                canNavigateToQuantityBySupervisors: this.authorizedUser.IsAdministrator || this.authorizedUser.IsHeadquarter,
                reportName: "Quantity",
                responsibleColumnName: PeriodicStatusReport.TeamMember,
                totalRowPresent: true,
                perTeam:false,
                supervisorId: supervisorId);

            model.ReportTypes = this.quantityReportTypesForSupervisor;
            model.SupervisorName = GetSupervisorName(supervisorId);

            return this.View("SpeedAndQuantity", model);
        }

        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
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
                perTeam: true,
                responsibleColumnName: PeriodicStatusReport.Team);

            model.ReportTypes = this.quantityReportTypesForHeadquarters;

            return this.View("SpeedAndQuantity", model);
        }

        [ActivePage(MenuItem.SpeedOfCompletingInterviews)]
        public ActionResult SpeedByInterviewers(Guid? supervisorId, PeriodiceReportType reportType = PeriodiceReportType.AverageInterviewDuration)
        {
            var periodicStatusReportWebApiActionName =
             reportTypesWhichShouldBeReroutedToCustomStatusController.Contains(reportType)
                 ? PeriodicStatusReportWebApiActionName.BetweenStatusesByInterviewers
                 : PeriodicStatusReportWebApiActionName.ByInterviewers;

            var model = this.CreatePeriodicStatusReportModel(
                reportType: reportType,
                webApiActionName: periodicStatusReportWebApiActionName,
                canNavigateToQuantityByTeamMember: false,
                canNavigateToQuantityBySupervisors: this.authorizedUser.IsAdministrator || this.authorizedUser.IsHeadquarter,
                reportName: "Speed",
                responsibleColumnName: PeriodicStatusReport.TeamMember,
                totalRowPresent: true,
                perTeam:false,
                supervisorId: supervisorId);

            model.ReportTypes = this.speedReportTypesForSupervisor;
            model.SupervisorName = GetSupervisorName(supervisorId);

            return this.View("SpeedAndQuantity", model);
        }

        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
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
                reportName: "Speed", totalRowPresent: true,
                perTeam: true,
                responsibleColumnName: PeriodicStatusReport.Team);

            model.ReportTypes = this.speedReportTypesForHeadquarters;

            return this.View("SpeedAndQuantity", model);
        }

        private PeriodicStatusReportModel CreatePeriodicStatusReportModel(PeriodicStatusReportWebApiActionName webApiActionName,
            PeriodiceReportType reportType,
            bool canNavigateToQuantityByTeamMember,
            bool canNavigateToQuantityBySupervisors,
            string reportName,
            string responsibleColumnName,
            bool totalRowPresent,
            bool perTeam,
            Guid? supervisorId = null)
        {
            var allUsersAndQuestionnaires = this.allUsersAndQuestionnairesFactory.Load();
            DateTime? utcMinAllowedDate = this.interviewStatuses.Query(_ => _.SelectMany(x => x.InterviewCommentedStatuses).Select(x => (DateTime?)x.Timestamp).Min());
            var localDate = utcMinAllowedDate?.ToLocalTime();

            return new PeriodicStatusReportModel
            {
                WebApiActionName = webApiActionName,
                CanNavigateToQuantityByTeamMember = canNavigateToQuantityByTeamMember,
                CanNavigateToQuantityBySupervisors = canNavigateToQuantityBySupervisors,
                Questionnaires = allUsersAndQuestionnaires.Questionnaires.ToArray(),
                ReportName = reportName,
                ResponsibleColumnName = responsibleColumnName,
                SupervisorId = supervisorId,
                ReportNameDescription = string.Format(GetReportDescriptionByType(supervisorId, reportType, perTeam), PeriodicStatusReport.Team.ToLower()),
                TotalRowPresent = totalRowPresent,
                MinAllowedDate = localDate ?? DateTime.Now
            };
        }

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

        private string GetReportDescriptionByType(Guid? supervisorId, PeriodiceReportType reportType, bool perTeam)
        {
            switch (reportType)
            {
                case PeriodiceReportType.NumberOfCompletedInterviews:
                    return perTeam ? PeriodicStatusReport.NumberOfCompletedInterviewsDescriptionPerTeam
                        : PeriodicStatusReport.NumberOfCompletedInterviewsDescriptionPerInterviewer;
                case PeriodiceReportType.NumberOfInterviewTransactionsBySupervisor:
                    return perTeam ? PeriodicStatusReport.NumberOfInterviewTransactionsBySupervisorDescriptionPerTeam
                        : PeriodicStatusReport.NumberOfInterviewTransactionsBySupervisorDescriptionPerInterviewer;
                case PeriodiceReportType.NumberOfInterviewTransactionsByHQ:
                    return supervisorId.HasValue ? 
                        PeriodicStatusReport.NumberOfCompletedInterviewsDescriptionPerTeam : 
                        PeriodicStatusReport.NumberOfInterviewTransactionsByHQDescription;
                case PeriodiceReportType.NumberOfInterviewsApprovedByHQ:
                    return supervisorId.HasValue ? 
                        PeriodicStatusReport.NumberOfCompletedInterviewsDescriptionPerTeam : 
                        PeriodicStatusReport.NumberOfInterviewsApprovedByHQDescription;

                case PeriodiceReportType.AverageCaseAssignmentDuration:
                    return PeriodicStatusReport.AverageCaseAssignmentDurationDescription;
                case PeriodiceReportType.AverageInterviewDuration:
                    return (supervisorId.HasValue || !perTeam)? 
                        PeriodicStatusReport.AverageInterviewDurationDescription : 
                        PeriodicStatusReport.AverageInterviewDurationForSupervisors;
                case PeriodiceReportType.AverageSupervisorProcessingTime:
                    return supervisorId.HasValue ? 
                        PeriodicStatusReport.AverageInterviewDurationDescription : 
                        PeriodicStatusReport.AverageSupervisorProcessingTimeDescription;
                case PeriodiceReportType.AverageHQProcessingTime:
                    return supervisorId.HasValue ? 
                        PeriodicStatusReport.AverageInterviewDurationDescription : 
                        PeriodicStatusReport.AverageHQProcessingTimeDescription;
                case PeriodiceReportType.AverageOverallCaseProcessingTime:
                    return supervisorId.HasValue ? 
                        PeriodicStatusReport.AverageInterviewDurationDescription : 
                        PeriodicStatusReport.AverageOverallCaseProcessingTimeDescription;
            }

            return string.Empty;
        }

        private string GetSupervisorName(Guid? supervisorId)
        {
            if (!supervisorId.HasValue)
                return null;

            var userView = this.userViewFactory.GetUser(new UserViewInputModel(supervisorId.Value));
            return userView?.UserName;
        }
    }
}
