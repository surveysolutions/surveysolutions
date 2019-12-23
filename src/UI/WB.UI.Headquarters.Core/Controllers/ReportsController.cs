using Microsoft.AspNetCore.Authorization;
﻿using System;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Reports;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IMapReport mapReport;

        public ReportsController(IMapReport mapReport)
        {
            this.mapReport = mapReport;
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.SurveyAndStatuses)]
        public ActionResult SurveysAndStatuses()
        {
            var model = new SurveysAndStatusesModel();
            model.DataUrl = Url.Action("HeadquarterSurveysAndStatusesReport", "ReportDataApi");
            model.InterviewsUrl = Url.Action("Index", "Interviews");
            model.ResponsiblesUrl = Url.Action("Supervisors", "UsersTypeahead");

            model.ReportName = Pages.SurveysAndStatuses_Overview;
            model.Subtitle = Pages.SurveysAndStatuses_HeadquartersDescription;

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
                DataUrl = Url.Action("DeviceInterviewers", "ReportDataApi"),
                InterviewersBaseUrl = Url.Action("Index", "Interviewers"),
                InterviewerProfileUrl = Url.Action("Profile", "Interviewer")
            };
            return this.View("InterviewersAndDevices", devicesInterviewersModel);
        }

    }
}
