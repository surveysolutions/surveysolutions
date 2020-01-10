using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    [ActivePage(MenuItem.Assignments)]
    [Localizable(false)]
    public class AssignmentsController : Controller
    {
        private readonly IAuthorizedUser currentUser;
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly IAssignmentsService assignments;
        private readonly IAssignmentViewFactory assignmentViewFactory;

        public AssignmentsController(IAuthorizedUser currentUser, 
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory, 
            IAssignmentsService assignments, 
            IAssignmentViewFactory assignmentViewFactory)
        {
            this.currentUser = currentUser;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.assignments = assignments;
            this.assignmentViewFactory = assignmentViewFactory;
        }
        
        [ActivePage(MenuItem.Assignments)]
        public IActionResult Index(int? id)
        {
            if (id.HasValue) return GetAssignmentDetails(id.Value);

            var questionnaires = this.allUsersAndQuestionnairesFactory.GetQuestionnaireComboboxViewItems();

            var model = new AssignmentsFilters 
            {
                IsSupervisor = this.currentUser.IsSupervisor,
                IsObserver = this.currentUser.IsObserver,
                IsObserving = this.currentUser.IsObserving,
                IsHeadquarter = this.currentUser.IsHeadquarter || this.currentUser.IsAdministrator,
                Questionnaires = questionnaires,
                MaxInterviewsByAssignment = Constants.MaxInterviewsCountByAssignment
            };

            model.Api = new AssignmentsFilters.ApiEndpoints
            {
                Responsible = model.IsSupervisor
                    ? Url.Action("InterviewersCombobox", "Teams")
                    : Url.Action("ResponsiblesCombobox", "Teams"),
                Assignments = Url.Action("Get", "AssignmentsApi"),
                Interviews = Url.Action("Index", "Interviews"),
                AssignmentsPage = Url.Action("Index", "Assignments"),
                Profile = Url.Action("Profile", "Interviewer"),
                SurveySetup = model.IsSupervisor ? "" : Url.Action("Index", "SurveySetup"),
                AssignmentsApi = Url.Content("~/api/v1/assignments")
            };

            return View(model);
        }

        private IActionResult GetAssignmentDetails(int assignmentId)
        {
            var assignment = this.assignments.GetAssignment(assignmentId);
            if (assignment == null) 
                return NotFound();

            return View("Details", new AssignmentDto
            {
                Archived = assignment.Archived,
                CreatedAtUtc = assignment.CreatedAtUtc,
                Email = assignment.Email,
                Id = assignment.Id,
                IdentifyingData = this.assignmentViewFactory.GetIdentifyingColumnText(assignment).Select(x =>
                    new AssignmentIdentifyingAnswerDto
                    {
                        Id = x.Identity.ToString(),
                        Title = x.Title,
                        Answer = x.Answer
                    }),
                InterviewsNeeded = assignment.InterviewsNeeded,
                InterviewsProvided = assignment.InterviewSummaries.Count,
                IsAudioRecordingEnabled = assignment.AudioRecording,
                IsCompleted = assignment.IsCompleted,
                Password = assignment.Password,
                ProtectedVariables = assignment.ProtectedVariables,
                Quantity = assignment.Quantity,
                Questionnaire = new AssignmentQuestionnaireDto
                {
                    Id = assignment.QuestionnaireId.QuestionnaireId,
                    Version = assignment.QuestionnaireId.Version,
                    Title = assignment.Questionnaire.Title
                },
                ReceivedByTabletAtUtc = assignment.ReceivedByTabletAtUtc,
                Responsible = new AssignmentResponsibleDto
                {
                    Id = assignment.ResponsibleId,
                    Name = assignment.Responsible.Name,
                    Role = Enum.GetName(typeof(UserRoles), assignment.Responsible.RoleIds.FirstOrDefault().ToUserRole())
                        ?.ToLower()
                },
                UpdatedAtUtc = assignment.UpdatedAtUtc,
                WebMode = assignment.WebMode,
                IsHeadquarters = this.currentUser.IsAdministrator || this.currentUser.IsHeadquarter,
                Comments = assignment.Comments
            });
        }
    }

    public class AssignmentDto
    {
        public int Id { get; set; }

        public AssignmentResponsibleDto Responsible { get; set; }

        public int? Quantity { get; set; }

        public bool Archived { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }

        public DateTime? ReceivedByTabletAtUtc { get; set; }

        public bool IsAudioRecordingEnabled { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public bool? WebMode { get; set; }

        public IEnumerable<AssignmentIdentifyingAnswerDto> IdentifyingData { get; set; }

        public AssignmentQuestionnaireDto Questionnaire { get; set; }

        public List<string> ProtectedVariables { get; set; }

        public int InterviewsProvided { get; set; }

        public int? InterviewsNeeded { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsHeadquarters { get; set; }
        public string Comments { get; set; }
        public string HistoryUrl { get; set; }
    }

    public class AssignmentQuestionnaireDto
    {
        public Guid Id { get; set; }
        public long Version { get; set; }
        public string Title { get; set; }
    }

    public class AssignmentResponsibleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }

    public class AssignmentIdentifyingAnswerDto
    {
        public string Title { get; set; }
        public string Answer { get; set; }
        public string Id { get; set; }
    }

    public class AssignmentsFilters
    {
        public bool IsHeadquarter { get; set; }
        public bool IsSupervisor { get; set; }
        public ApiEndpoints Api { get; set; }
        public bool IsObserver { get; set; }
        public bool IsObserving { get; set; }
        public List<QuestionnaireVersionsComboboxViewItem> Questionnaires { get; set; }
        public int MaxInterviewsByAssignment { get; set; }

        public class ApiEndpoints
        {
            public string Assignments { get; set; }

            public string Profile { get; set; }
            public string Interviews { get; set; }
            public string Responsible { get; set; }

            public string SurveySetup { get; set; }
            public string AssignmentsPage { get; set; }
            public string AssignmentsApi { get; set; }
        }
    }
}
