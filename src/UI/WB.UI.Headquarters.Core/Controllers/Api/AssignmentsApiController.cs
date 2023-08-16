using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Sanitizer;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Route("api/{controller}/{action=Get}")]
    public class AssignmentsApiController : ControllerBase
    {
        private readonly IAssignmentViewFactory assignmentViewFactory;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IAssignmentsService assignmentsStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ISystemLog auditLog;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires;
        private readonly IInvitationService invitationService;
        private readonly IStatefulInterviewRepository interviews;
        private readonly IAssignmentPasswordGenerator passwordGenerator;
        private readonly ICommandService commandService;
        private readonly IAssignmentFactory assignmentFactory;

        public AssignmentsApiController(IAssignmentViewFactory assignmentViewFactory,
            IAuthorizedUser authorizedUser,
            IAssignmentsService assignmentsStorage,
            IQuestionnaireStorage questionnaireStorage,
            ISystemLog auditLog,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires, 
            IInvitationService invitationService,
            IStatefulInterviewRepository interviews, 
            IAssignmentPasswordGenerator passwordGenerator,
            ICommandService commandService,
            IAssignmentFactory assignmentFactory)
        {
            this.assignmentViewFactory = assignmentViewFactory;
            this.authorizedUser = authorizedUser;
            this.assignmentsStorage = assignmentsStorage;
            this.questionnaireStorage = questionnaireStorage;
            this.auditLog = auditLog;
            this.questionnaires = questionnaires;
            this.invitationService = invitationService;
            this.interviews = interviews;
            this.passwordGenerator = passwordGenerator;
            this.commandService = commandService;
            this.assignmentFactory = assignmentFactory;
        }
        
        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor, Interviewer")]
        public ActionResult<AssignmetsDataTableResponse> Get(AssignmentsDataTableRequest request)
        {
            var isInterviewer = this.authorizedUser.IsInterviewer;
                       
            var input = new AssignmentsInputModel
            {
                Limit = request.PageSize,
                Offset = (request.PageIndex - 1) * request.PageSize,
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Order = request.GetSortOrder(),
                SearchBy = request.Search?.Value,
                QuestionnaireId = request.QuestionnaireId,
                QuestionnaireVersion = request.QuestionnaireVersion,
                ResponsibleId = isInterviewer ? this.authorizedUser.Id :request.ResponsibleId,
                ShowArchive = !isInterviewer && request.ShowArchive,
                DateStart = request.DateStart?.ToUniversalTime(),
                DateEnd = request.DateEnd?.ToUniversalTime(),
                UserRole = request.UserRole,
                ReceivedByTablet = request.ReceivedByTablet,
                SupervisorId = request.TeamId,
                Id = request.Id
            };

            if (this.authorizedUser.IsSupervisor)
            {
                input.SupervisorId = this.authorizedUser.Id;
            }

            if (isInterviewer)
            {
                input.OnlyWithInterviewsNeeded = true;
                input.SearchByFields = AssignmentsInputModel.SearchTypes.Id 
                    | AssignmentsInputModel.SearchTypes.IdentifyingQuestions
                    | AssignmentsInputModel.SearchTypes.QuestionnaireTitle;
                input.ShowQuestionnaireTitle = true;
                input.NonCawiOnly = true;
            }

            var result = this.assignmentViewFactory.Load(input);
            var response = new AssignmetsDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = result.TotalCount,
                RecordsFiltered = result.TotalCount,
                Data = result.Items
            };
            return this.Ok(response);
        }

        
        [HttpDelete]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObservingNotAllowed]
        [ActionName("Get")]
        public IActionResult Delete([FromBody]int[] ids)
        {
            if (ids == null) return this.BadRequest();

            try
            {
                foreach (var id in ids)
                {
                    Assignment assignment = this.assignmentsStorage.GetAssignment(id);
                    commandService.Execute(new ArchiveAssignment(assignment.PublicKey, authorizedUser.Id, assignment.QuestionnaireId));
                }
                
                return this.Ok();
            }
            catch (AssignmentException e)
            {
                return this.BadRequest(new {Message = e.Message});
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObservingNotAllowed]
        public IActionResult Unarchive([FromBody]int[] ids)
        {
            if (ids == null) return this.BadRequest();
            
            try
            {
                foreach (var id in ids)
                {
                    Assignment assignment = this.assignmentsStorage.GetAssignment(id);
                    commandService.Execute(new UnarchiveAssignment(assignment.PublicKey, authorizedUser.Id, assignment.QuestionnaireId));
                }

                return this.Ok();
            }
            catch (AssignmentException e)
            {
                return this.BadRequest(new {Message = e.Message});
            }
        }

        [HttpPost]
        [ObservingNotAllowed]
        public IActionResult Assign([FromBody] AssignRequest request)
        {
            if (request?.Ids == null) return this.BadRequest();
            try
            {
                foreach (var idToAssign in request.Ids)
                {
                    Assignment assignment = this.assignmentsStorage.GetAssignment(idToAssign);
                    commandService.Execute(
                    new ReassignAssignment(assignment.PublicKey, authorizedUser.Id, 
                        request.ResponsibleId, request.Comments, assignment.QuestionnaireId));

                    if (!string.IsNullOrEmpty(request.Comments))
                        assignment.SetComments(request.Comments);
                }

                return this.Ok();
            }
            catch (AssignmentException e)
            {
                return this.BadRequest(new {Message = e.Message});
            }
        }

        [HttpPost]
        [ObservingNotAllowed]
        public IActionResult Create([FromBody] CreateAssignmentRequest request)
        {
            if (!this.authorizedUser.IsAdministrator && !this.authorizedUser.IsHeadquarter)
                return Forbid();

            if (request == null)
                return this.BadRequest();

            var interview = this.interviews.Get(request.InterviewId);
            if (interview == null)
                return this.NotFound();

            int? quantity;
            switch (request.Quantity)
            {
                case null:
                    quantity = 1;
                    break;
                case -1:
                    quantity = null;
                    break;
                default:
                    quantity = request.Quantity;
                    break;
            }

            var password = passwordGenerator.GetPassword(request.Password);

            //verify email
            if (!string.IsNullOrEmpty(request.Email) && AssignmentConstants.EmailRegex.Match(request.Email).Length <= 0)
                return this.BadRequest(new {Message = "Invalid Email"});

            //verify pass
            if (!string.IsNullOrEmpty(password))
            {
                if ((password.Length < AssignmentConstants.PasswordLength ||
                     AssignmentConstants.PasswordStrength.Match(password).Length <= 0))
                    this.BadRequest(new {Message = "Invalid Password. At least 6 numbers and upper case letters or single symbol '?' to generate password"});
            }

            //assignment with email must have quantity = 1
            if (!string.IsNullOrEmpty(request.Email) && request.Quantity != 1)
                this.BadRequest(new {Message = "For assignments with provided email allowed quantity is 1"});

            if ((!string.IsNullOrEmpty(request.Email) || !string.IsNullOrEmpty(password)) && request.WebMode != true)
                this.BadRequest(new {Message = "For assignments having Email or Password Web Mode should be activated"});

            if (quantity == 1 && (request.WebMode == null || request.WebMode == true) &&
                string.IsNullOrEmpty(request.Email) && !string.IsNullOrEmpty(password))
            {
                var hasPasswordInDb = this.assignmentsStorage.DoesExistPasswordInDb(interview.QuestionnaireIdentity, password);

                if (hasPasswordInDb)
                    return this.BadRequest(new {Message = Assignments.DuplicatePasswordByWebModeWithQuantity1});
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, null);
            
            var answers = Assignment.GetAnswersFromInterview(interview, questionnaire);
            bool isAudioRecordingEnabled = request.IsAudioRecordingEnabled ?? this.questionnaires.Query(_ => _
                                               .Where(q => q.Id == interview.QuestionnaireIdentity.ToString())
                                               .Select(q => q.IsAudioRecordingEnabled).FirstOrDefault());

            try
            {

                var assignment = assignmentFactory.CreateAssignment(authorizedUser.Id,
                interview.QuestionnaireIdentity,
                request.ResponsibleId,
                request.Quantity,
                request.Email,
                password,
                request.WebMode,
                isAudioRecordingEnabled,
                answers,
                null,
                request.Comments);
                
                this.invitationService.CreateInvitationForWebInterview(assignment);
                
                return this.Ok();
            }
            catch (AssignmentException e)
            {
                return this.BadRequest(new {Message = e.Message});
            }
        }

        public class CreateAssignmentRequest
        {
            public string InterviewId { get; set; }
            public Guid ResponsibleId { get; set; }
            public int? Quantity { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public bool? WebMode { get; set; }
            public bool? IsAudioRecordingEnabled { get; set; }
            public string Comments { get; set; }
        }

        public class UpdateAssignmentRequest
        {
            public int? Quantity { get; set; }
        }

        public class AssignRequest
        {
            public Guid ResponsibleId { get; set; }
            public string Comments { get; set; }

            public int[] Ids { get; set; }
        }

        public class AssignmetsDataTableResponse : DataTableResponse<AssignmentRow>
        {
        }

        public class AssignmentsDataTableRequest : DataTableRequest
        {
            public Guid? QuestionnaireId { get; set; }
            public long? QuestionnaireVersion { get; set; }
            public Guid? ResponsibleId { get; set; }
            public Guid? TeamId { get; set; }

            public bool ShowArchive { get; set; }

            public DateTime? DateStart { get; set; }
            public DateTime? DateEnd { get; set; }
            public UserRoles? UserRole { get; set; }
            public AssignmentReceivedState ReceivedByTablet { get; set; }

            public int? Id { get; set; }
        }
        
        [HttpPost]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer)]
        public AssignmentForMapPointView AssignmentMapPoint([FromBody]AssignmentForMapPointViewModel data)
        {
            return data == null ? null : GetAssignmentForMapPointView(data.AssignmentId);
        }

        private AssignmentForMapPointView GetAssignmentForMapPointView(int assignmentId)
        {
            var assignment = this.assignmentsStorage.GetAssignment(assignmentId);
            if (assignment == null)
                return null;

            var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(assignment.QuestionnaireId, null);

            var mapPointView = new AssignmentForMapPointView
            {
                ResponsibleName = assignment.Responsible.Name,
                AssignmentId = assignment.Id,
                Quantity = assignment.Quantity,
                InterviewsNeeded = assignment.InterviewsNeeded,
                LastUpdatedDate = AnswerUtils.AnswerToString(assignment.UpdatedAtUtc),
                IdentifyingData = assignment.IdentifyingData
                    .Where(d => questionnaire.GetQuestionType(d.Identity.Id) != QuestionType.GpsCoordinates)
                    .Select(d =>
                    new AnswerView()
                    {
                        Title = questionnaire.GetQuestionTitle(d.Identity.Id).RemoveHtmlTags(),
                        Answer = d.AnswerAsString.RemoveHtmlTags(),
                    })
                    .ToList()
            };
            return mapPointView;
        }

        public class AssignmentForMapPointViewModel
        {
            public int AssignmentId { get; set; }
        }
        
        public class AssignmentForMapPointView
        {
            public string ResponsibleName { get; set; }
            public int? Quantity { get; set; }
            public int? InterviewsNeeded { get; set; }
            public string LastUpdatedDate { get; set; }
            public int AssignmentId { get; set; }
            public List<AnswerView> IdentifyingData { get; set; }
        }
    }
}
