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


        

        
        

        
    }
}
