using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Models.Reports;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IMapReport mapReport;
        private readonly IChartStatisticsViewFactory chartStatisticsViewFactory;
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses;
        private readonly IUserViewFactory userViewFactory;
        private readonly IAuthorizedUser authorizedUser;

        public ReportsController(IMapReport mapReport, 
            IChartStatisticsViewFactory chartStatisticsViewFactory, 
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses,
            IUserViewFactory userViewFactory, 
            IAuthorizedUser authorizedUser)
        {
            this.mapReport = mapReport;
            this.chartStatisticsViewFactory = chartStatisticsViewFactory;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.interviewStatuses = interviewStatuses;
            this.userViewFactory = userViewFactory;
            this.authorizedUser = authorizedUser;
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.SurveyAndStatuses)]
        public ActionResult SurveysAndStatuses()
        {
            var model = new SurveysAndStatusesModel();
            model.DataUrl = Url.Action("HeadquarterSurveysAndStatusesReport", "ReportDataApi");
            model.InterviewsUrl = Url.Action("Index", "Interviews");
            model.ResponsiblesUrl = Url.Action("Supervisors", "UsersTypeahead");
            model.SelfUrl = Url.Action("SurveysAndStatuses");
            return this.View(model);
        }

        [Authorize(Roles = "Supervisor")]
        [ActivePage(MenuItem.SurveyAndStatuses)]
        public ActionResult SurveysAndStatusesForSv()
        {
            var model = new SurveysAndStatusesModel();
            model.DataUrl = Url.Action("SupervisorSurveysAndStatusesReport", "ReportDataApi");
            model.InterviewsUrl = Url.Action("Index", "Interviews");
            model.ResponsiblesUrl = Url.Action("AsigneeInterviewersBySupervisor", "UsersTypeahead");
            model.SelfUrl = Url.Action("SurveysAndStatusesForSv");

            return this.View(model);
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.Summary)]
        public ActionResult SupervisorsAndStatuses()
        {
            var model = new TeamsAndStatusesModel
            {
                DataUrl = Url.Action("HeadquarterSupervisorsAndStatusesReport", "ReportDataApi"),
                QuestionnairesUrl = Url.Action("QuestionnairesWithVersions", "QuestionnairesApi"),
                QuestionnaireByIdUrl = Url.Action("QuestionnairesComboboxById", "QuestionnairesApi"),
                InterviewsUrl = Url.Action("Index", "Interviews"),
                AllTeamsTitle = Strings.AllTeams,
                TeamTitle = Users.Supervisors,
                ReportName = Reports.TeamsAndStatuses,
                Subtitle = Reports.TeamsAndStatuses_HeadquartersSubtitle
            };
            return this.View("TeamsAndStatuses", model);
        }


        [Authorize(Roles = "Supervisor")]
        [ActivePage(MenuItem.Summary)]
        public ActionResult TeamMembersAndStatuses()
        {
            var model = new TeamsAndStatusesModel
            {
                DataUrl = Url.Action("SupervisorTeamMembersAndStatusesReport", "ReportDataApi"),
                QuestionnairesUrl = Url.Action("QuestionnairesWithVersions", "QuestionnairesApi"),
                QuestionnaireByIdUrl = Url.Action("QuestionnairesComboboxById", "QuestionnairesApi"),
                InterviewsUrl = Url.Action("Index", "Interviews"),
                AllTeamsTitle = Strings.AllInterviewers,
                TeamTitle = Pages.TeamMember,
                ReportName = Reports.Report_Team_Members_and_Statuses,
                Subtitle = Reports.TeamsAndStatuses_SupervisorSubtitle,
                IsSupervisorMode = true
            };
            return this.View(model);
        }

        [Authorize(Roles = "Administrator, Supervisor, Headquarter")]
        [ActivePage(MenuItem.MapReport)]
        [ExtraHeaderPermissions(HeaderPermissionType.Google)]
        public ActionResult MapReport()
        {
            var questionnaires = this.mapReport.GetQuestionnaireIdentitiesWithGpsQuestions();

            return this.View(new
            {
                Questionnaires = questionnaires.GetQuestionnaireComboboxViewItems()
            });
        }


        [ActivePage(MenuItem.DevicesInterviewers)]
        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult InterviewersAndDevices(Guid? id)
        {
            var devicesInterviewersModel = new DevicesInterviewersModel
            {
                BaseReportUrl = Url.Action("InterviewersAndDevices", "Reports", new { id = (Guid?)null }),
                DataUrl = Url.Action("DeviceInterviewers", "ReportDataApi"),
                InterviewersBaseUrl = Url.Action("Interviewers", "Users"),
                InterviewerProfileUrl = Url.Action("Profile", "Interviewer"),
                SupervisorProfileUrl = Url.Action("Manage", "Users"),
            };
            return this.View("InterviewersAndDevices", devicesInterviewersModel);
        }

        [ActivePage(MenuItem.InterviewsChart)]
        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult InterviewsChart()
        {
            var questionnaires = this.chartStatisticsViewFactory.GetQuestionnaireListWithData();

            return this.View("CumulativeInterviewChart", new
            {
                Templates = questionnaires
            });
        }

        [ActivePage(MenuItem.NumberOfCompletedInterviews)]
        public ActionResult QuantityByInterviewers(Guid? supervisorId, PeriodiceReportType reportType = PeriodiceReportType.NumberOfCompletedInterviews)
        {
            var model = this.CreatePeriodicStatusReportModel(
                reportType: reportType,
                webApiActionName: PeriodicStatusReportWebApiActionName.ByInterviewers,
                canNavigateToQuantityByTeamMember: false,
                canNavigateToQuantityBySupervisors: this.authorizedUser.IsAdministrator || this.authorizedUser.IsHeadquarter,
                reportName: "Quantity",
                responsibleColumnName: PeriodicStatusReport.TeamMember,
                totalRowPresent: true,
                perTeam: false,
                supervisorId: supervisorId);

            model.ReportTypes = ToComboboxItems(this.quantityReportTypesForSupervisor);
            model.SupervisorName = GetSupervisorName(supervisorId);

            return this.View("SpeedAndQuantity", model);
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.NumberOfCompletedInterviews)]
        public ActionResult QuantityBySupervisors(PeriodiceReportType reportType = PeriodiceReportType.NumberOfCompletedInterviews)
        {
            var model = this.CreatePeriodicStatusReportModel(
                reportType: reportType,
                webApiActionName: PeriodicStatusReportWebApiActionName.BySupervisors,
                canNavigateToQuantityByTeamMember: true,
                canNavigateToQuantityBySupervisors: false,
                reportName: "Quantity",
                totalRowPresent: true,
                perTeam: true,
                responsibleColumnName: PeriodicStatusReport.Team);

            model.ReportTypes = ToComboboxItems(this.quantityReportTypesForHeadquarters);

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
                perTeam: false,
                supervisorId: supervisorId);

            model.ReportTypes = ToComboboxItems(this.speedReportTypesForSupervisor);
            model.SupervisorName = GetSupervisorName(supervisorId);

            return this.View("SpeedAndQuantity", model);
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.SpeedOfCompletingInterviews)]
        public ActionResult SpeedBySupervisors(PeriodiceReportType reportType = PeriodiceReportType.AverageInterviewDuration)
        {
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

            model.ReportTypes = ToComboboxItems(this.speedReportTypesForHeadquarters);

            return this.View("SpeedAndQuantity", model);
        }

        [ActivePage(MenuItem.SurveyStatistics)]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult SurveyStatistics()
        {
            var statuses = StatusHelper.GetSurveyStatisticsStatusItems(this.authorizedUser).ToList();

            return this.View("SurveyStatistics", new
            {
                statuses,
                isSupervisor = this.authorizedUser.IsSupervisor
            });
        }

        [ActivePage(MenuItem.StatusDuration)]
        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult StatusDuration()
        {
            return this.View("StatusDuration", new StatusDurationModel
            {
                DataUrl = Url.Action("StatusDuration", "ReportDataApi"),
                InterviewsBaseUrl = Url.Action("Index", "Interviews"),
                AssignmentsBaseUrl = Url.Action("Index", "Assignments"),
                QuestionnairesUrl = Url.Action("QuestionnairesWithVersions", "QuestionnairesApi"),
                QuestionnaireByIdUrl = Url.Action("QuestionnairesComboboxById", "QuestionnairesApi")
            });
        }

        [ActivePage(MenuItem.StatusDuration)]
        [Authorize(Roles = "Supervisor")]
        public ActionResult TeamStatusDuration()
        {
            return this.View("StatusDuration", new StatusDurationModel
            {
                IsSupervisorMode = true,

                DataUrl = Url.Action("TeamStatusDuration", "ReportDataApi"),
                InterviewsBaseUrl = Url.Action("Index", "Interviews"),
                AssignmentsBaseUrl = Url.Action("Index", "Assignments"),
                QuestionnairesUrl = Url.Action("QuestionnairesWithVersions", "QuestionnairesApi"),
                QuestionnaireByIdUrl = Url.Action("QuestionnairesComboboxById", "QuestionnairesApi")
            });
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
                DataUrl = Url.Action(reportName + webApiActionName, "ReportDataApi"),
                CanNavigateToQuantityByTeamMember = canNavigateToQuantityByTeamMember,
                CanNavigateToQuantityBySupervisors = canNavigateToQuantityBySupervisors,
                Questionnaires = ToComboboxItems(allUsersAndQuestionnaires.Questionnaires),
                ReportName = reportName,
                ResponsibleColumnName = responsibleColumnName,
                SupervisorId = supervisorId,
                ReportNameDescription = string.Format(GetReportDescriptionByType(supervisorId, reportType, perTeam), PeriodicStatusReport.Team.ToLower()),
                TotalRowPresent = totalRowPresent,
                MinAllowedDate = (localDate ?? DateTime.Now).ToString("s"),
                SupervisorsUrl = Url.Action(reportName + "BySupervisors", "Reports"),
                InterviewersUrl = Url.Action(reportName + "ByInterviewers", "Reports"),
                Periods = new[]
                {
                    new ComboboxViewItem() { Key = "d", Value = PeriodicStatusReport.Day   },
                    new ComboboxViewItem() { Key = "w", Value = PeriodicStatusReport.Week  },
                    new ComboboxViewItem() { Key = "m", Value = PeriodicStatusReport.Month },
                },
                OverTheLasts = Enumerable.Range(1, 12).Select(i => new ComboboxViewItem() { Value = i.ToString(), Key = i.ToString() }).ToArray()
            };
        }

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
                    return (supervisorId.HasValue || !perTeam) ?
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

        private ComboboxViewItem[] ToComboboxItems(PeriodiceReportType[] types)
        {
            return types
                .Select(type => new ComboboxViewItem()
                {
                    Key = ((int)type).ToString(),
                    Value = PeriodicStatusReport.ResourceManager.GetString(type.ToString()),
                })
                .ToArray();
        }

        private QuestionnaireVersionsComboboxViewItem[] ToComboboxItems(IEnumerable<TemplateViewItem> questionnaires)
        {
            return questionnaires
                .GroupBy(x => new { x.TemplateId, x.TemplateName })
                .Select(x => new QuestionnaireVersionsComboboxViewItem()
                {
                    Key = x.Key.TemplateId.FormatGuid(),
                    Value = x.Key.TemplateName,
                    Versions = x.Select(y => new ComboboxViewItem()
                    {
                        Key = y.TemplateVersion.ToString(),
                        Value = y.TemplateVersion.ToString(),
                    }).OrderByDescending(y => y.Value).ToList()
                })
                .OrderBy(x => x.Value)
                .ToArray();
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
