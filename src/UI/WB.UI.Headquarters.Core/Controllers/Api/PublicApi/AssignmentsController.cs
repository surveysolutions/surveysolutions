#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mime;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;
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
        private readonly IUserViewFactory userViewFactory;
        private readonly IAssignmentsImportService assignmentsImportService;
        private readonly ISerializer serializer;
        private readonly IPreloadedDataVerifier verifier;
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUnitOfWork unitOfWork;
        private readonly IInvitationService invitationService;
        private readonly IWebInterviewLinkProvider interviewLinkProvider;
        private readonly IInScopeExecutor inScopeExecutor;

        public AssignmentsController(
            IAssignmentViewFactory assignmentViewFactory,
            IAssignmentsService assignmentsStorage,
            IMapper mapper,
            IUserRepository userManager,
            IQuestionnaireStorage questionnaireStorage,
            ISystemLog auditLog,
            IPreloadedDataVerifier verifier,
            ICommandService commandService,
            IAuthorizedUser authorizedUser,
            IUnitOfWork unitOfWork,
            IUserViewFactory userViewFactory,
            IAssignmentsImportService assignmentsImportService,
            ISerializer serializer,
            IInvitationService invitationService,
            IWebInterviewLinkProvider interviewLinkProvider,
            IInScopeExecutor inScopeExecutor)
        {
            this.assignmentViewFactory = assignmentViewFactory;
            this.assignmentsStorage = assignmentsStorage;
            this.mapper = mapper;
            this.userManager = userManager;
            this.questionnaireStorage = questionnaireStorage;
            this.auditLog = auditLog;
            this.verifier = verifier;
            this.commandService = commandService;
            this.authorizedUser = authorizedUser;
            this.unitOfWork = unitOfWork;
            this.userViewFactory = userViewFactory;
            this.assignmentsImportService = assignmentsImportService;
            this.serializer = serializer;
            this.invitationService = invitationService;
            this.interviewLinkProvider = interviewLinkProvider;
            this.inScopeExecutor = inScopeExecutor;
        }

        /// <summary>
        /// Single assignment details
        /// </summary>
        /// <response code="200">Assignment details</response>
        /// <response code="404">Assignment cannot be found</response>
        [HttpGet]
        [Route("{id:int}")]
        [Produces(MediaTypeNames.Application.Json)]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult<FullAssignmentDetails> Details(int id)
        {
            Assignment assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null)
                return NotFound();

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(assignment.QuestionnaireId, null);

            var result = this.mapper.Map<FullAssignmentDetails>(assignment, o => o.Items["questionnaire"] = questionnaire);
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
        [Produces(MediaTypeNames.Application.Json)]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public async Task<ActionResult<AssignmentsListView>> List([FromQuery] AssignmentsListFilter? filter)
        {
            filter ??= new AssignmentsListFilter
            {
                Offset = 0,
                Limit = 20
            };

            filter.Limit = filter.Limit == 0 ? 20 : Math.Min(filter.Limit, 100);

            if (!QuestionnaireIdentity.TryParse(filter.QuestionnaireId, out QuestionnaireIdentity? questionnaireId))
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
                    SupervisorId = filter.SupervisorId
                });

                var listView = new AssignmentsListView(result.Page, result.PageSize, result.TotalCount, filter.Order)
                    {
                        Assignments = this.mapper.Map<List<AssignmentViewItem>>(result.Items)
                    };

                return listView;
            }
            catch (NotSupportedException)
            {
                unitOfWork.DiscardChanges();
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }

            string? MapOrder(string? input)
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
        /// <response code="201">Created assignment with details</response>
        /// <response code="400">Bad parameters provided or identifying data incorrect. See response details for more info</response>
        /// <response code="404">Questionnaire not found</response>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("")]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public ActionResult<CreateAssignmentResult> Create(
            [FromBody, BindRequired] CreateAssignmentApiRequest? createItem)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, 
                    $@"Invalid parameter or property: {string.Join(',',ModelState.Keys.ToList())}");
            
            if (createItem == null) return StatusCode(StatusCodes.Status400BadRequest, "Bad assignment info");

            if (!QuestionnaireIdentity.TryParse(createItem.QuestionnaireId, out QuestionnaireIdentity questionnaireId))
            {
                return StatusCode(StatusCodes.Status404NotFound,
                    $"Questionnaire not found: {createItem.QuestionnaireId}");
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireId, null);
            if (questionnaire == null)
                return StatusCode(StatusCodes.Status404NotFound,
                    $"Questionnaire not found: {createItem.QuestionnaireId}");

            if (string.IsNullOrEmpty(createItem.Responsible))
                return StatusCode(StatusCodes.Status400BadRequest, "Responsible is required");

            try
            {
                var assignmentAnswers = createItem.IdentifyingData
                .Select(x => x.ToAssignmentAnswer(questionnaire))
                .ToList();

            var unknownQuestions = assignmentAnswers
                .Where(x => x.IsUnknownQuestion)
                .ToArray();

            if (unknownQuestions.Any())
                return StatusCode(StatusCodes.Status400BadRequest,
                    $"Question(s) not found: {string.Join(", ", unknownQuestions.Select(x => x.Source.Variable ?? x.Source.Identity))}");

            var assignmentRows = createItem.ToAssignmentRows(assignmentAnswers, 
                questionnaire, userViewFactory, serializer).ToList();

            var verificationErrors = VerifyAssignment(assignmentRows, questionnaire).ToList();
            if (verificationErrors.Any())
            {
                return StatusCode(StatusCodes.Status400BadRequest, new CreateAssignmentResult
                {
                    VerificationStatus = new ImportDataVerificationState {Errors = verificationErrors}
                });
            }

            var assignmentToImport =
                this.assignmentsImportService.ConvertToAssignmentToImport(assignmentRows, questionnaire, createItem.ProtectedVariables);

            var importError = this.verifier.VerifyWithInterviewTree(assignmentToImport.Answers,
                this.authorizedUser.Id, questionnaire);

            if (importError != null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new CreateAssignmentResult
                {
                    VerificationStatus = new ImportDataVerificationState
                    {
                        Errors = new[]
                        {
                            new PanelImportVerificationError(importError.ErrorCode, importError.ErrorMessage)
                        }.ToList()
                    }
                });
            }

            var assignmentId = this.assignmentsImportService.ImportAssignment(
                assignmentToImport, questionnaire, this.authorizedUser.Id);

            var assignment = this.assignmentsStorage.GetAssignment(assignmentId);

            var result = new CreateAssignmentResult
            {
                Assignment = mapper.Map<AssignmentDetails>(assignment)
            };

            if (assignment?.WebMode == true)
            {
                var invitation = this.invitationService.GetInvitationByAssignmentId(assignment.Id);

                if (invitation != null)
                {
                    result.WebInterviewLink = this.interviewLinkProvider.WebInterviewStartLink(invitation);
                }
            }
            
            return CreatedAtAction("Details", new {id = result.Assignment.Id}, result);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid data.");
            }
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
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator)]
        public async Task<ActionResult<AssignmentDetails>> Assign(int id,
            [FromBody, BindRequired] AssignmentAssignRequest? assigneeRequest)
        {
            if (assigneeRequest == null) return StatusCode(StatusCodes.Status400BadRequest, "User was not set");
            
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, 
                    $@"Invalid parameter or property: {string.Join(',',ModelState.Keys.ToList())}");

            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null)
            {
                return NotFound();
            }

            var responsibleUser = await this.GetResponsibleIdPersonFromRequestValueAsync(assigneeRequest.Responsible);
            if (responsibleUser == null)
            {
                return NotFound("User was not found");
            }
            
            try
            {
                this.VerifyAssigneeInRoles(responsibleUser, assigneeRequest.Responsible, UserRoles.Interviewer,
                    UserRoles.Supervisor);
            }
            catch (HttpException e)
            {
                unitOfWork.DiscardChanges();
                return StatusCode(e.StatusCode, e.Message);
            }

            await commandService.ExecuteAsync(new ReassignAssignment(assignment.PublicKey, authorizedUser.Id, responsibleUser.Id,
                assignment.Comments, assignment.QuestionnaireId));

            return GetUpdatedAssignment(id);
        }

        private AssignmentDetails GetUpdatedAssignment(int id)
        {
            var updatedDetails =
                inScopeExecutor.Execute(sl =>
                {
                    var assignmentsService = sl.GetInstance<IAssignmentsService>();
                    var assignment = assignmentsService.GetAssignment(id);

                    return this.mapper.Map<AssignmentDetails>(assignment);
                });
            return updatedDetails;
        }

        private void VerifyAssigneeInRoles(HqUser? responsibleUser, string? providedValue, params UserRoles[] roles)
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

        private async Task<HqUser?> GetResponsibleIdPersonFromRequestValueAsync(string responsible)
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
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Headquarter, UserRoles.Administrator)]
        [ObservingNotAllowed]
        public ActionResult<AssignmentDetails> ChangeQuantity(int id, [FromBody] int quantity)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null)
            {
                return NotFound();
            }

            if (assignment.WebMode == true)
                return StatusCode(StatusCodes.Status406NotAcceptable, Assignments.WebMode);

            commandService.Execute(
                new UpdateAssignmentQuantity(assignment.PublicKey, authorizedUser.Id, quantity, assignment.QuestionnaireId));
            this.auditLog.AssignmentSizeChanged(id, quantity);

            return GetUpdatedAssignment(id);
        }

        /// <summary>
        /// Archive assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <response code="200">Assignment details</response>
        /// <response code="404">Assignment not found</response>
        [HttpPatch]
        [Route("{id:int}/archive")]
        [Produces(MediaTypeNames.Application.Json)]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Headquarter, UserRoles.Administrator)]
        [ObservingNotAllowed]
        public ActionResult<AssignmentDetails> Archive(int id)
        {
            var assignment = assignmentsStorage.GetAssignment(id);

            if (assignment == null)
            {
                return NotFound();
            }

            commandService.Execute(new ArchiveAssignment(assignment.PublicKey, authorizedUser.Id, assignment.QuestionnaireId));

            return GetUpdatedAssignment(id);
        }

        /// <summary>
        /// Unarchive assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <response code="200">Assignment details</response>
        /// <response code="404">Assignment not found</response>
        [HttpPatch]
        [Route("{id:int}/unarchive")]
        [Produces(MediaTypeNames.Application.Json)]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Headquarter, UserRoles.Administrator)]
        [ObservingNotAllowed]
        public ActionResult<AssignmentDetails> Unarchive(int id)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null)
                return NotFound();

            commandService.Execute(
                new UnarchiveAssignment(assignment.PublicKey, authorizedUser.Id, assignment.QuestionnaireId));

            return GetUpdatedAssignment(id);
        }

        /// <summary>
        /// Get status of audio recording for provided assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <response code="200"></response>
        /// <response code="404">Assignment not found</response>
        [HttpGet]
        [Route("{id:int}/recordAudio")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Headquarter, UserRoles.Administrator)]
        public ActionResult<AudioRecordingEnabled> AudioRecoding(int id)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null || assignment.Archived)
            {
                return NotFound();
            }

            return new AudioRecordingEnabled
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
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Headquarter, UserRoles.Administrator)]
        public ActionResult AudioRecodingPatch(int id, [FromBody] UpdateRecordingRequest request)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, 
                    $@"Invalid parameter or property: {string.Join(',',ModelState.Keys.ToList())}");
            
            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null || assignment.Archived)
                return NotFound();

            commandService.Execute(
                new UpdateAssignmentAudioRecording(assignment.PublicKey, authorizedUser.Id, 
                    request.Enabled, assignment.QuestionnaireId));

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
        [Produces(MediaTypeNames.Application.Json)]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Headquarter, UserRoles.Administrator)]
        public ActionResult<AssignmentQuantitySettings> AssignmentQuantitySettings(int id)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null || assignment.Archived)
                return NotFound();

            return new AssignmentQuantitySettings
            {
                CanChangeQuantity = assignment.QuantityCanBeChanged
            };
        }

        [HttpPost]
        [Obsolete("Use PATCH method instead")]
        [Route("{id:int}/close")]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Headquarter, UserRoles.Administrator)]
        public ActionResult ClosePost(int id)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null)
                return NotFound();
            if (!assignment.QuantityCanBeChanged)
                return Conflict();

            this.commandService.Execute(new UpdateAssignmentQuantity(assignment.PublicKey,
                this.authorizedUser.Id,
                assignment.InterviewSummaries.Count,
                assignment.QuestionnaireId));

            return Ok();
        }

        /// <summary>
        /// Close assignment by setting Size to the amount of collected interviews
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <response code="200">Assignment closed</response>
        /// <response code="404">Assignment not found</response>
        /// <response code="409">Quantity cannot be changed. Assignment either archived or has web mode enabled</response>
        [HttpPatch]
        [Route("{id:int}/close")]
        [Produces(MediaTypeNames.Application.Json)]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Headquarter, UserRoles.Administrator)]
        public ActionResult<AssignmentDetails> Close(int id)
        {
            var assignment = assignmentsStorage.GetAssignment(id);
            if (assignment == null)
                return NotFound();
            if (!assignment.QuantityCanBeChanged)
                return Conflict();

            this.commandService.Execute(new UpdateAssignmentQuantity(assignment.PublicKey,
                this.authorizedUser.Id,
                assignment.InterviewSummaries.Count,
                assignment.QuestionnaireId));

            return GetUpdatedAssignment(id);
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
        [Produces(MediaTypeNames.Application.Json)]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Supervisor, UserRoles.Headquarter, UserRoles.Administrator)]
        public async Task<ActionResult<AssignmentHistory>> History(int id, [FromQuery] int start = 0,
            [FromQuery] int length = 30)
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
                if (responsible.WorkspaceProfile.SupervisorId != this.authorizedUser.Id)
                    return Forbid();
            }

            AssignmentHistory result =
                await this.assignmentViewFactory.LoadHistoryAsync(assignment.PublicKey, start, length);
            return result;
        }


        private IEnumerable<PanelImportVerificationError> VerifyAssignment(List<PreloadingAssignmentRow> assignmentRows,
            IQuestionnaire questionnaire)
        {
            foreach (var assignmentRow in assignmentRows)
            foreach (var error in this.verifier.VerifyRowValues(assignmentRow, questionnaire))
                yield return error;

            foreach (var error in this.verifier.VerifyRosters(assignmentRows, questionnaire))
                yield return error;

            foreach (var error in this.verifier.VerifyWebPasswords(assignmentRows, questionnaire))
                yield return error;
        }
    }
}
