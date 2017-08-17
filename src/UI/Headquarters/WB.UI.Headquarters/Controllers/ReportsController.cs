using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.ComponentModels;
using WB.UI.Headquarters.Models.Reports;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Utils;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    public class ReportsController : Controller
    {
        private readonly IMapReport mapReport;
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUserViewFactory userViewFactory;
        private readonly ITeamUsersAndQuestionnairesFactory teamUsersAndQuestionnairesFactory;
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses;

        public ReportsController(
            IMapReport mapReport, 
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory, 
            IAuthorizedUser authorizedUser, 
            IUserViewFactory userViewFactory, 
            ITeamUsersAndQuestionnairesFactory teamUsersAndQuestionnairesFactory, 
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses)
        {
            this.mapReport = mapReport;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.authorizedUser = authorizedUser;
            this.userViewFactory = userViewFactory;
            this.teamUsersAndQuestionnairesFactory = teamUsersAndQuestionnairesFactory;
            this.interviewStatuses = interviewStatuses;
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult SurveysAndStatuses()
        {
            this.ViewBag.ActivePage = MenuItem.Surveys;

            return this.View();
        }

        [Authorize(Roles = "Supervisor")]
        public ActionResult SurveysAndStatusesForSv()
        {
            this.ViewBag.ActivePage = MenuItem.Surveys;

            return this.View();
        }


        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult SupervisorsAndStatuses()
        {
            this.ViewBag.ActivePage = MenuItem.Summary;

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load();

            return this.View("TeamsAndStatuses", usersAndQuestionnaires.Questionnaires);
        }


        [Authorize(Roles = "Supervisor")]
        public ActionResult TeamMembersAndStatuses()
        {
            this.ViewBag.ActivePage = MenuItem.Summary;
            TeamUsersAndQuestionnairesView usersAndQuestionnaires =
                this.teamUsersAndQuestionnairesFactory.Load(new TeamUsersAndQuestionnairesInputModel(this.authorizedUser.Id));
            return this.View(usersAndQuestionnaires.Questionnaires);
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult MapReport()
        {
            this.ViewBag.ActivePage = MenuItem.MapReport;

            var questionnaires = this.mapReport.GetQuestionnaireIdentitiesWithPoints();

            return this.View(new MapReportModel
            {
                Questionnaires = new ComboboxModel(questionnaires.Select(x => new ComboboxOptionModel(x.Id, $"(ver. {x.Version}) {x.Title}")).ToArray(), questionnaires.Count)
            });
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult InterviewsChart()
        {
            this.ViewBag.ActivePage = MenuItem.InterviewsChart;

            IEnumerable<SurveyStatusViewItem> statuses = StatusHelper.GetOnlyActualSurveyStatusViewItems(this.authorizedUser.IsSupervisor);

            AllUsersAndQuestionnairesView usersAndQuestionnaires =
                this.allUsersAndQuestionnairesFactory.Load();

            return this.View("CumulativeInterviewChart", new DocumentFilter
            {
                Templates = usersAndQuestionnaires.Questionnaires,
                Statuses = statuses
            });
        }

        [ActivePage(MenuItem.CountDaysOfInterviewInStatus)]
        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult CountDaysOfInterviewInStatus()
        {
            return this.View("CountDaysOfInterviewInStatus", new CountDaysOfInterviewInStatusModel
            {
                BasePath = Url.Content(@"~/"),
                DataUrl = Url.RouteUrl("DefaultApiWithAction",
                    new
                    {
                        httproute = "",
                        controller = "ReportDataApi",
                        action = "CountDaysOfInterviewInStatus"
                    }),
                InterviewsBaseUrl = Url.Action("Interviews", "HQ"),
                AssignmentsBaseUrl = Url.Action("Index", "Assignments"),
                Questionnaires = this.GetQuestionnaires(),

                Resources = new[]
                {
                    Strings.ResourceManager,
                    Pages.ResourceManager
                }.Translations()
            });
        }

        private ComboboxOptionModel[] GetQuestionnaires()
        {
            AllUsersAndQuestionnairesView usersAndQuestionnaires = this.allUsersAndQuestionnairesFactory.Load();

            return usersAndQuestionnaires.Questionnaires.Select(s => new ComboboxOptionModel(
                new QuestionnaireIdentity(s.TemplateId, s.TemplateVersion).ToString(),
                $@"(ver. {s.TemplateVersion.ToString()}) {s.TemplateName}")).ToArray();
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
                supervisorId: supervisorId);

            model.ReportTypes = this.quantityReportTypesForSupervisor;
            model.SupervisorName = GetSupervisorName(supervisorId);

            return this.View("SpeedAndQuantity", model);
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

            return this.View("SpeedAndQuantity", model);
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
                canNavigateToQuantityBySupervisors: this.authorizedUser.IsAdministrator || this.authorizedUser.IsHeadquarter,
                reportName: "Speed",
                responsibleColumnName: PeriodicStatusReport.TeamMember,
                totalRowPresent: false,
                supervisorId: supervisorId);

            model.ReportTypes = this.speedReportTypesForSupervisor;
            model.SupervisorName = GetSupervisorName(supervisorId);

            return this.View("SpeedAndQuantity", model);
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

            return this.View("SpeedAndQuantity", model);
        }

        [ActivePage(MenuItem.DevicesInterviewers)]
        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult InterviewersAndDevices()
        {
            return this.View("InterviewersAndDevices", new DevicesInterviewersModel
            {
                BasePath = Url.Content(@"~/"),
                DataUrl = Url.RouteUrl("DefaultApiWithAction", 
                new
                {
                    httproute = "",
                    controller = "ReportDataApi",
                    action = "DeviceInterviewers"
                }),
                InterviewersBaseUrl = Url.Action("Index", "Interviewers"),
                Resources = new[]
                {
                    DevicesInterviewers.ResourceManager,
                    Pages.ResourceManager
                }.Translations()
            });
        }

        private PeriodicStatusReportModel CreatePeriodicStatusReportModel(PeriodicStatusReportWebApiActionName webApiActionName, 
            PeriodiceReportType reportType,
            bool canNavigateToQuantityByTeamMember, 
            bool canNavigateToQuantityBySupervisors, 
            string reportName, 
            string responsibleColumnName, 
            bool totalRowPresent, 
            Guid? supervisorId = null)
        {
            var allUsersAndQuestionnaires = this.allUsersAndQuestionnairesFactory.Load();
            DateTime? minAllowedDate = this.interviewStatuses.Query(_ => _.SelectMany(x => x.InterviewCommentedStatuses).Select(x => (DateTime?)x.Timestamp).Min());

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
                TotalRowPresent = totalRowPresent,
                MinAllowedDate = minAllowedDate ?? DateTime.Now
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

            var userView = this.userViewFactory.GetUser(new UserViewInputModel(supervisorId.Value));
            return userView?.UserName;
        }
    }
}