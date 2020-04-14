using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Exceptions;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Route("api/v1/assignments")]
    [Localizable(false)]
    [PublicApiJson]
    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentsService assignmentsStorage;
        private readonly IAssignmentViewFactory assignmentViewFactory;
        private readonly IMapper mapper;
        private readonly IUserRepository userManager;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ISystemLog auditLog;
        private readonly ICommandTransformator commandTransformator;
        private readonly IInterviewCreatorFromAssignment interviewCreatorFromAssignment;
        private readonly IPreloadedDataVerifier verifier;
        private readonly IAssignmentFactory assignmentFactory;
        private readonly IInvitationService invitationService;
        private readonly IAssignmentPasswordGenerator passwordGenerator;
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUnitOfWork unitOfWork;

        public AssignmentsController(
            IAssignmentViewFactory assignmentViewFactory,
            IAssignmentsService assignmentsStorage,
            IMapper mapper,
            IUserRepository userManager,
            IQuestionnaireStorage questionnaireStorage,
            ISystemLog auditLog,
            IInterviewCreatorFromAssignment interviewCreatorFromAssignment,
            IPreloadedDataVerifier verifier,
            IAssignmentFactory assignmentFactory,
            IInvitationService invitationService,
            IAssignmentPasswordGenerator passwordGenerator,
            ICommandService commandService,
            IAuthorizedUser authorizedUser,
            IUnitOfWork unitOfWork,
            ICommandTransformator commandTransformator)
        {
            this.assignmentViewFactory = assignmentViewFactory;
            this.assignmentsStorage = assignmentsStorage;
            this.mapper = mapper;
            this.userManager = userManager;
            this.questionnaireStorage = questionnaireStorage;
            this.auditLog = auditLog;
            this.interviewCreatorFromAssignment = interviewCreatorFromAssignment;
            this.verifier = verifier;
            this.assignmentFactory = assignmentFactory;
            this.invitationService = invitationService;
            this.passwordGenerator = passwordGenerator;
            this.commandService = commandService;
            this.authorizedUser = authorizedUser;
            this.unitOfWork = unitOfWork;
            this.commandTransformator = commandTransformator;
        }

        /// <summary>
        /// Single assignment details
        /// </summary>
        /// <response code="200">Assignment details</response>
        /// <response code="404">Assignment cannot be found</response>
        [HttpGet]
        [Route("{id:int}")]
        [Authorize(Roles = "ApiUser, Administrator")]
        public ActionResult<FullAssignmentDetails> Details(int id)
        {
            Assignment assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null)
                return NotFound();
            var result = this.mapper.Map<FullAssignmentDetails>(assignment);
            return result;
        }

        /// <summary>
        /// List all assignments with filtering
        /// </summary>
        /// <param name="filter">List filter options</param>
        /// <returns>List of assignments</returns>
        /// <returns code="406">Incorrect filtering data provided</returns>
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "ApiUser, Administrator")]
        public async Task<ActionResult<AssignmentsListView>> List([FromQuery] AssignmentsListFilter filter)
        {
            filter ??= new AssignmentsListFilter
            {
                Offset = 0,
                Limit = 20
            };

            filter.Limit = filter.Limit == 0 ? 20 : Math.Min(filter.Limit, 100);

            if (!QuestionnaireIdentity.TryParse(filter.QuestionnaireId, out QuestionnaireIdentity questionnaireId))
            {
                questionnaireId = null;
            }

            var responsible = await GetResponsibleIdPersonFromRequestValueAsync(filter.Responsible);
            try
            {

                AssignmentsWithoutIdentifingData result = this.assignmentViewFactory.Load(new AssignmentsInputModel
                {
                    QuestionnaireId = questionnaireId?.QuestionnaireId,
                    QuestionnaireVersion = questionnaireId?.Version,
                    ResponsibleId = responsible?.Id,
                    Order = MapOrder(filter.Order),
                    Limit = filter.Limit,
                    Offset = filter.Offset,
                    SearchBy = filter.SearchBy,
                    ShowArchive = filter.ShowArchive,
                    SupervisorId = filter.SupervisorId,
                });

                var listView = new AssignmentsListView(result.Page, result.PageSize, result.TotalCount, filter.Order);

                listView.Assignments = this.mapper.Map<List<AssignmentViewItem>>(result.Items);
                return listView;
            }
            catch (NotSupportedException)
            {
                unitOfWork.DiscardChanges();
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }

            string MapOrder(string input)
            {
                if (input == null) return null;

                var order = input.Split(' ');
                var column = order[0];

                var direction = order.Length > 1 ? order[1] : @"ASC";

                switch (column.ToLower())
                {
                    case "id": return $"Id {direction}";
                    case "responsiblename": return $"Responsible.Name {direction}";
                    case "interviewscount": return $"InterviewsCount {direction}";
                    case "quantity": return $"Quantity {direction}";
                    case "updatedatutc": return $"UpdatedAtUtc {direction}";
                    case "createdatutc": return $"CreatedAtUtc {direction}";
                }

                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Create new assignment
        /// </summary>
        /// <param name="createItem">New assignments options</param>
        /// <response code="200">Created assignment with details</response>
        /// <response code="400">Bad parameters provided or identifying data incorrect. See response details for more info</response>
        /// <response code="404">Questionnaire or responsible user not found</response>
        /// <response code="406">Responsible user provided in request cannot be assigned to assignment</response>
        [HttpPost]
        [Authorize(Roles = "ApiUser, Administrator")]
        [Route("")]
        //[ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true)]
        public async Task<ActionResult<CreateAssignmentResult>> Create([FromBody] CreateAssignmentApiRequest createItem)
        {
            var responsible = await this.GetResponsibleIdPersonFromRequestValueAsync(createItem?.Responsible);

            try
            {
                this.VerifyAssigneeInRoles(responsible, createItem?.Responsible, UserRoles.Interviewer, UserRoles.Supervisor);
            }
            catch (HttpException e)
            {
                return StatusCode(e.StatusCode, e.Message);
            }

            if (!QuestionnaireIdentity.TryParse(createItem.QuestionnaireId, out QuestionnaireIdentity questionnaireId))
            {
                return StatusCode(StatusCodes.Status404NotFound,
                    $"Questionnaire not found: {createItem?.QuestionnaireId}");
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireId, null);

            if (questionnaire == null)
                return StatusCode(StatusCodes.Status404NotFound,
                    $"Questionnaire not found: {createItem?.QuestionnaireId}");

            int? quantity;
            switch (createItem.Quantity)
            {
                case null:
                    quantity = 1;
                    break;
                case -1:
                    quantity = null;
                    break;
                default:
                    quantity = createItem.Quantity;
                    break;
            }

            var password = passwordGenerator.GetPassword(createItem.Password);

            //verify email
            if (!string.IsNullOrEmpty(createItem.Email) && AssignmentConstants.EmailRegex.Match(createItem.Email).Length <= 0)
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid Email");

            //verify pass
            if (!string.IsNullOrEmpty(password))
            {
                if ((password.Length < AssignmentConstants.PasswordLength ||
                     AssignmentConstants.PasswordStrength.Match(password).Length <= 0))
                    return StatusCode(StatusCodes.Status400BadRequest,
                        "Invalid Password. At least 6 numbers and upper case letters or single symbol '?' to generate password");
            }

            //assignment with email must have quantity = 1
            if (!string.IsNullOrEmpty(createItem.Email) && createItem.Quantity != 1)
                return StatusCode(StatusCodes.Status400BadRequest,
                    "For assignments with provided email allowed quantity is 1");

            if ((!string.IsNullOrEmpty(createItem.Email) || !string.IsNullOrEmpty(password)) && createItem.WebMode != true)
                return StatusCode(StatusCodes.Status400BadRequest,
                    "For assignments having Email or Password Web Mode should be activated");

            if (createItem.Quantity == 1 && createItem.WebMode &&
                string.IsNullOrEmpty(createItem.Email) && !string.IsNullOrEmpty(createItem.Password))
            {
                var hasPasswordInDb = this.assignmentsStorage.DoesExistPasswordInDb(questionnaireId, password);
                if (hasPasswordInDb)
                    return StatusCode(StatusCodes.Status400BadRequest,
                        "Password is not unique. Password by assignment for web mode with quantity 1 should be unique");
            }

            List<InterviewAnswer> answers = new List<InterviewAnswer>();

            foreach (var item in createItem.IdentifyingData)
            {
                Identity identity;
                try
                {
                    identity = string.IsNullOrEmpty(item.Identity)
                        ? new Identity(questionnaire.GetQuestionIdByVariable(item.Variable).Value, RosterVector.Empty)
                        : Identity.Parse(item.Identity);
                }
                catch (Exception ae)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        (string.IsNullOrEmpty(item.Identity)
                            ? "Question Identity cannot be parsed. Expected format: GuidWithoutDashes_Int1-Int2, where _Int1-Int2 - codes of parent rosters (empty if question is not inside any roster). For example: 11111111111111111111111111111111_0-1 should be used for question on the second level"
                            : "Question cannot be identified by provided variable name") +
                        Environment.NewLine +
                        ae.Message);
                }
                KeyValuePair<Guid, AbstractAnswer>? answer = null;
                try
                {
                    answer = this.commandTransformator.ParseQuestionAnswer(new UntypedQuestionAnswer
                    {
                        Id = identity.Id,
                        Answer = item.Answer,
                        Type = questionnaire.GetQuestionType(identity.Id)
                    }, questionnaire);
                }
                catch (Exception ae)
                {
                    unitOfWork.DiscardChanges();
                    return StatusCode(StatusCodes.Status400BadRequest,
                        $"Answer '{item.Answer}' cannot be parsed for question with Identity '{item.Identity}' and variable '{item.Variable}'." +
                        Environment.NewLine +
                        ae.Message);
                }

                answers.Add(new InterviewAnswer
                {
                    Identity = identity,
                    Answer = answer.Value.Value
                });
            }

            var assignment = this.assignmentFactory.CreateAssignment(authorizedUser.Id, questionnaireId, responsible.Id, quantity,
                createItem.Email, password, createItem.WebMode, createItem.IsAudioRecordingEnabled,
                answers, protectedVariables: null, createItem.Comments);

            var result = verifier.VerifyWithInterviewTree(answers, responsible.Id, questionnaire);

            if (result != null)
            {
                unitOfWork.DiscardChanges();
                return StatusCode(StatusCodes.Status400BadRequest, new CreateAssignmentResult
                {
                    Assignment = mapper.Map<AssignmentDetails>(assignment),
                    VerificationStatus = new ImportDataVerificationState
                    {
                        Errors = new List<PanelImportVerificationError>
                        {
                            new PanelImportVerificationError("PL0011", result.ErrorMessage)
                        }
                    }
                });
            }

            interviewCreatorFromAssignment.CreateInterviewIfQuestionnaireIsOld(responsible.Id, questionnaireId, assignment.Id, answers);
            assignment = this.assignmentsStorage.GetAssignmentByAggregateRootId(assignment.PublicKey);

            this.invitationService.CreateInvitationForWebInterview(assignment);

            return new CreateAssignmentResult
            {
                Assignment = mapper.Map<AssignmentDetails>(assignment)
            };
        }

        /// <summary>
        /// Assign new responsible person for assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <param name="assigneeRequest">Responsible user id or name</param>
        /// <response code="200">Assignment details with updated assignee</response>
        /// <response code="404">Assignment or assignee not found</response>
        /// <response code="406">Assignee cannot be assigned to assignment</response>
        [HttpPatch]
        [Route("{id:int}/assign")]
        [Authorize(Roles = "ApiUser, Administrator")]
        public async Task<ActionResult<AssignmentDetails>> Assign(int id, [FromBody] AssignmentAssignRequest assigneeRequest)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null)
            {
                return NotFound();
            }

            var responsibleUser = await this.GetResponsibleIdPersonFromRequestValueAsync(assigneeRequest?.Responsible);

            try
            {
                this.VerifyAssigneeInRoles(responsibleUser, assigneeRequest?.Responsible, UserRoles.Interviewer,
                    UserRoles.Supervisor);
            }
            catch (HttpException e)
            {
                unitOfWork.DiscardChanges();
                return StatusCode(e.StatusCode, e.Message);
            }

            commandService.Execute(new ReassignAssignment(assignment.PublicKey, authorizedUser.Id, responsibleUser.Id, assignment.Comments));

            return this.mapper.Map<AssignmentDetails>(assignmentsStorage.GetAssignment(id));
        }

        private void VerifyAssigneeInRoles(HqUser responsibleUser, string providedValue, params UserRoles[] roles)
        {
            if (responsibleUser == null)
            {
                throw new HttpException(HttpStatusCode.NotFound,
                    $@"User not found: {providedValue}");
            }

            if (!roles.Any(responsibleUser.IsInRole))
            {
                throw new HttpException(HttpStatusCode.NotAcceptable);
            }
        }

        private async Task<HqUser> GetResponsibleIdPersonFromRequestValueAsync(string responsible)
        {
            if (string.IsNullOrWhiteSpace(responsible))
            {
                return null;
            }

            return Guid.TryParse(responsible, out Guid responsibleGuid)
                ? await this.userManager.FindByIdAsync(responsibleGuid)
                : await this.userManager.FindByNameAsync(responsible);
        }

        /// <summary>
        /// Change assignments limit on created interviews
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <param name="quantity">New limit on created interviews</param>
        /// <response code="200">Assignment details with updated quantity</response>
        /// <response code="404">Assignment not found</response>
        /// <response code="406">Size cannot be changed</response>
        [HttpPatch]
        [Route("{id:int}/changeQuantity")]
        [Authorize(Roles = "ApiUser, Administrator, Headquarter")]
        public ActionResult<AssignmentDetails> ChangeQuantity(int id, [FromBody] int quantity)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null)
            {
                return NotFound();
            }

            if (assignment.WebMode == true)
                return StatusCode(StatusCodes.Status406NotAcceptable, Assignments.WebMode);

            commandService.Execute(new UpdateAssignmentQuantity(assignment.PublicKey, authorizedUser.Id, quantity));
            this.auditLog.AssignmentSizeChanged(id, quantity);

            var changedAssignment = assignmentsStorage.GetAssignment(id);
            var assignmentDetails = this.mapper.Map<AssignmentDetails>(changedAssignment);
            assignmentDetails.Quantity = quantity; // quantity changed in other transaction and is not visible from this unit of work.
            return assignmentDetails;
        }

        /// <summary>
        /// Archive assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <response code="200">Assignment details</response>
        /// <response code="404">Assignment not found</response>
        [HttpPatch]
        [Route("{id:int}/archive")]
        [Authorize(Roles = "ApiUser, Administrator, Headquarter")]
        public ActionResult<AssignmentDetails> Archive(int id)
        {
            var assignment = assignmentsStorage.GetAssignment(id);

            if (assignment == null)
            {
                return NotFound();
            }

            commandService.Execute(new ArchiveAssignment(assignment.PublicKey, authorizedUser.Id));

            return this.mapper.Map<AssignmentDetails>(assignmentsStorage.GetAssignment(id));
        }

        /// <summary>
        /// Archive assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <response code="200">Assingment details</response>
        /// <response code="404">Assignment not found</response>
        [HttpPatch]
        [Route("{id:int}/unarchive")]
        [Authorize(Roles = "ApiUser, Administrator, Headquarter")]
        public ActionResult<AssignmentDetails> Unarchive(int id)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null)
                return NotFound();

            commandService.Execute(new UnarchiveAssignment(assignment.PublicKey, authorizedUser.Id));

            return this.mapper.Map<AssignmentDetails>(assignmentsStorage.GetAssignment(id));
        }

        /// <summary>
        /// Gets status of audio recording for provided assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <response code="200"></response>
        /// <response code="404">Assignment not found</response>
        [HttpGet]
        [Route("{id:int}/recordAudio")]
        [Authorize(Roles = "ApiUser, Headquarter, Administrator")]
        public ActionResult<AssignmentAudioRecordingEnabled> AudioRecoding(int id)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null || assignment.Archived)
            {
                return NotFound();
            }

            return new AssignmentAudioRecordingEnabled
            {
                Enabled = assignment.AudioRecording
            };
        }

        /// <summary>
        /// Set audio recording setting for assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <param name="request"></param>
        /// <response code="204">Audio recording updated</response>
        /// <response code="404">Assignment not found</response>
        [HttpPatch]
        [Route("{id:int}/recordAudio")]
        [ObserverNotAllowed]
        [Authorize(Roles = "ApiUser, Headquarter, Administrator")]
        public ActionResult AudioRecodingPatch(int id, [FromBody] UpdateRecordingRequest request)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if(assignment == null || assignment.Archived) 
                return NotFound();

            commandService.Execute(new UpdateAssignmentAudioRecording(assignment.PublicKey, authorizedUser.Id, request.Enabled));

            return NoContent();
        }

        /// <summary>
        /// Gets Quantity Settings for provided assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <response code="200"></response>
        /// <response code="404">Assignment not found</response>
        [HttpGet]
        [Route("{id:int}/assignmentQuantitySettings")]
        [Authorize(Roles = "ApiUser, Headquarter, Administrator")]
        public ActionResult<AssignmentQuantitySettings> AssignmentQuantitySettings(int id)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if(assignment == null || assignment.Archived) 
                return NotFound();

            return new AssignmentQuantitySettings
            {
                CanChangeQuantity = assignment.QuantityCanBeChanged
            };
        }

        /// <summary>
        /// Closes assignment by setting Size to the amount of collected interviews
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <response code="200">Assignment closed</response>
        /// <response code="404">Assignment not found</response>
        /// <response code="409">Quantity cannot be changed. Assignment either archived or has web mode enabled</response>
        [HttpPost]
        [Route("{id:int}/close")]
        [ObserverNotAllowed]
        [Authorize(Roles = "ApiUser, Headquarter, Administrator")]
        public ActionResult Close(int id)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if(assignment == null) 
                return NotFound();
            if (!assignment.QuantityCanBeChanged)
                return Conflict();

            this.commandService.Execute(new UpdateAssignmentQuantity(assignment.PublicKey,
                this.authorizedUser.Id,
                assignment.InterviewSummaries.Count));

            return Ok();
        }

        /// <summary>
        /// Gets history of the assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <param name="start"></param>
        /// <param name="length">Limit of events to return</param>
        /// <response code="200">Assignment history</response>
        /// <response code="403">Assignment cannot accessed by logged in user</response>
        /// <response code="404">Assignment cannot be found</response>
        [HttpGet]
        [Route("{id:int}/history")]
        [Authorize(Roles = "ApiUser, Supervisor, Headquarter, Administrator")]
        public async Task<ActionResult<AssignmentHistory>> History(int id, [FromQuery]int start = 0, [FromQuery]int length = 30)
        {
            var assignment = this.assignmentsStorage.GetAssignment(id);
            if (assignment == null)
            {
                return NotFound();
            }

            if (this.authorizedUser.IsSupervisor && assignment.ResponsibleId != this.authorizedUser.Id)
            {
                var responsible = await this.userManager.FindByIdAsync(assignment.ResponsibleId);
                if (!responsible.IsInRole(UserRoles.Interviewer))
                    return Forbid();
                if(responsible.Profile.SupervisorId != this.authorizedUser.Id)
                    return Forbid();
            }

            AssignmentHistory result = await this.assignmentViewFactory.LoadHistoryAsync(assignment.PublicKey, start, length);
            return result;
        }
    }
}
