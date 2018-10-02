using AutoMapper;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.CommandTransformation;

namespace WB.UI.Headquarters.API.PublicApi
{
    [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true)]
    [RoutePrefix("api/v1/assignments")]
    public class AssignmentsController : BaseApiServiceController
    {
        private readonly IPlainStorageAccessor<Assignment> assignmentsStorage;
        private readonly IAssignmentViewFactory assignmentViewFactory;
        private readonly IMapper mapper;
        private readonly HqUserManager userManager;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IAuditLog auditLog;
        private readonly IInterviewCreatorFromAssignment interviewCreatorFromAssignment;
        private readonly IPreloadedDataVerifier verifier;
        private readonly ICommandTransformator commandTransformator;

        public AssignmentsController(
            IAssignmentViewFactory assignmentViewFactory,
            IPlainStorageAccessor<Assignment> assignmentsStorage,
            IMapper mapper,
            HqUserManager userManager,
            ILogger logger,
            IQuestionnaireStorage questionnaireStorage,
            IAuditLog auditLog,
            IInterviewCreatorFromAssignment interviewCreatorFromAssignment,
            IPreloadedDataVerifier verifier,
            ICommandTransformator commandTransformator) : base(logger)
        {
            this.assignmentViewFactory = assignmentViewFactory;
            this.assignmentsStorage = assignmentsStorage;
            this.mapper = mapper;
            this.userManager = userManager;
            this.questionnaireStorage = questionnaireStorage;
            this.auditLog = auditLog;
            this.interviewCreatorFromAssignment = interviewCreatorFromAssignment;
            this.verifier = verifier;
            this.commandTransformator = commandTransformator;
        }

        /// <summary>
        /// Single assignment details
        /// </summary>
        /// <response code="200">Assignment details</response>
        /// <response code="404">Assignment cannot be found</response>
        [HttpGet]
        [Route("{id:int}")]
        public FullAssignmentDetails Details(int id)
        {
            Assignment assignment = assignmentsStorage.GetById(id)
                ?? throw new HttpResponseException(HttpStatusCode.NotFound);

            return this.mapper.Map<FullAssignmentDetails>(assignment);
        }

        /// <summary>
        /// List all assignments with filtering
        /// </summary>
        /// <param name="filter">List filter options</param>
        /// <returns>List of assignments</returns>
        /// <returns code="406">Incorrect filtering data provided</returns>
        [HttpGet]
        [Route("")]
        [Localizable(false)]
        public AssignmentsListView List([FromUri(SuppressPrefixCheck = true, Name = "")] AssignmentsListFilter filter)
        {
            filter = filter ?? new AssignmentsListFilter
            {
                Offset = 0,
                Limit = 20
            };

            filter.Limit = filter.Limit == 0 ? 20 : Math.Min(filter.Limit, 100);

            if (!QuestionnaireIdentity.TryParse(filter.QuestionnaireId, out QuestionnaireIdentity questionnaireId))
            {
                questionnaireId = null;
            }

            var responsible = GetResponsibleIdPersonFromRequestValue(filter.Responsible);

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

                throw new HttpResponseException(HttpStatusCode.NotAcceptable);
            }
        }

        /// <summary>
        /// Create new assignment
        /// </summary>
        /// <param name="createItem">New assignments options</param>
        /// <response code="200">Created assignment with details</response>
        /// <response code="400">Bad parameters provided or identifiying data incorrect. See response details for more info</response>
        /// <response code="404">Questionnaire or responsible user not found</response>
        /// <response code="406">Responsible user provided in request cannot be assigned to assignment</response>
        [HttpPost]
        [Route]
        public CreateAssignmentResult Create(CreateAssignmentApiRequest createItem)
        {
            var responsible = this.GetResponsibleIdPersonFromRequestValue(createItem?.Responsible);

            this.VerifyAssigneeInRoles(responsible, createItem?.Responsible, UserRoles.Interviewer, UserRoles.Supervisor);

            if (!QuestionnaireIdentity.TryParse(createItem.QuestionnaireId, out QuestionnaireIdentity questionnaireId))
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, $@"Questionnaire not found: {createItem?.QuestionnaireId}"));
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireId, null);

            if (questionnaire == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, $@"Questionnaire not found: {createItem?.QuestionnaireId}"));

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

            var assignment = new Assignment(questionnaireId, responsible.Id, quantity);

            var identifyingQuestionIds = questionnaire.GetPrefilledQuestions().ToHashSet();


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
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest,
                        (string.IsNullOrEmpty(item.Identity)
                            ? "Question Identity cannot be parsed. Expected format: GuidWithoutDashes_Int1-Int2, where _Int1-Int2 - codes of parent rosters (empty if question is not inside any roster). For example: 11111111111111111111111111111111_0-1 should be used for question on the second level"
                            : "Question cannot be identified by provided variable name") +
                        Environment.NewLine +
                        ae.Message));
                }
                KeyValuePair<Guid, AbstractAnswer> answer;
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
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest,
                        $"Answer '{item.Answer}' canot be parsed for question with Identity '{item.Identity}' and variable '{item.Variable}'." +
                        Environment.NewLine +
                        ae.Message));
                }

                answers.Add(new InterviewAnswer
                {
                    Identity = identity,
                    Answer = answer.Value
                });
            }

            List<IdentifyingAnswer> identifyingAnswers =
                answers.Where(x => identifyingQuestionIds.Contains(x.Identity.Id))
                    .Select(a => IdentifyingAnswer.Create(assignment, questionnaire, a.Answer.ToString(), a.Identity))
                    .ToList();
            assignment.SetIdentifyingData(identifyingAnswers);
            assignment.SetAnswers(answers);

            var result = verifier.VerifyWithInterviewTree(answers, responsible.Id, questionnaire);

            if (result != null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest, new CreateAssignmentResult
                {
                    Assignment = mapper.Map<AssignmentDetails>(assignment),
                    VerificationStatus = new ImportDataVerificationState
                    {
                        Errors = new List<PanelImportVerificationError>
                        {
                            new PanelImportVerificationError("PL0011", result.ErrorMessage)
                        }
                    }
                }));
            }

            this.assignmentsStorage.Store(assignment, null);
            interviewCreatorFromAssignment.CreateInterviewIfQuestionnaireIsOld(responsible.Id, questionnaireId,
                assignment.Id, answers);
            assignment = this.assignmentsStorage.GetById(assignment.Id);

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
        public AssignmentDetails Assign(int id, [FromBody] AssignmentAssignRequest assigneeRequest)
        {
            var assignment = assignmentsStorage.GetById(id) ??
            throw new HttpResponseException(HttpStatusCode.NotFound);

            var responsibleUser = this.GetResponsibleIdPersonFromRequestValue(assigneeRequest?.Responsible);

            this.VerifyAssigneeInRoles(responsibleUser, assigneeRequest?.Responsible, UserRoles.Interviewer,
                UserRoles.Supervisor);

            assignment.Reassign(responsibleUser.Id);

            assignmentsStorage.Store(assignment, id);

            return this.mapper.Map<AssignmentDetails>(assignmentsStorage.GetById(id));
        }

        private void VerifyAssigneeInRoles(HqUser responsibleUser, string providedValue, params UserRoles[] roles)
        {
            if (responsibleUser == null)
            {
                throw new HttpResponseException(this.Request.CreateResponse(HttpStatusCode.NotFound,
                    $@"User not found: {providedValue}"));
            }

            if (!roles.Any(responsibleUser.IsInRole))
            {
                throw new HttpResponseException(HttpStatusCode.NotAcceptable);
            }
        }

        private HqUser GetResponsibleIdPersonFromRequestValue(string responsible)
        {
            if (string.IsNullOrWhiteSpace(responsible))
            {
                return null;
            }

            return Guid.TryParse(responsible, out Guid responsibleUserId)
                ? this.userManager.FindById(responsibleUserId)
                : this.userManager.FindByName(responsible);
        }

        /// <summary>
        /// Change assignments limit on created interviews
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <param name="quantity">New limit on created interviews</param>
        /// <response code="200">Assingment details with updated quantity</response>
        /// <response code="404">Assignment not found</response>
        [HttpPatch]
        [Route("{id:int}/changeQuantity")]
        public AssignmentDetails ChangeQuantity(int id, [FromBody] int? quantity)
        {
            var assignment = assignmentsStorage.GetById(id) ?? throw new HttpResponseException(HttpStatusCode.NotFound);

            assignment.UpdateQuantity(quantity);
            assignmentsStorage.Store(assignment, id);
            this.auditLog.AssignmentSizeChanged(id, quantity);

            return this.mapper.Map<AssignmentDetails>(assignmentsStorage.GetById(id));
        }

        /// <summary>
        /// Archive assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <response code="200">Assingment details</response>
        /// <response code="404">Assignment not found</response>
        [HttpPatch]
        [Route("{id:int}/archive")]
        public AssignmentDetails Archive(int id)
        {
            var assignment = assignmentsStorage.GetById(id) ?? throw new HttpResponseException(HttpStatusCode.NotFound);

            assignment.Archive();

            assignmentsStorage.Store(assignment, id);

            return this.mapper.Map<AssignmentDetails>(assignmentsStorage.GetById(id));
        }

        /// <summary>
        /// Archive assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <response code="200">Assingment details</response>
        /// <response code="404">Assignment not found</response>
        [HttpPatch]
        [Route("{id:int}/unarchive")]
        public AssignmentDetails Unarchive(int id)
        {
            var assignment = assignmentsStorage.GetById(id) ?? throw new HttpResponseException(HttpStatusCode.NotFound);

            assignment.Unarchive();

            assignmentsStorage.Store(assignment, id);

            return this.mapper.Map<AssignmentDetails>(assignmentsStorage.GetById(id));
        }
    }
}
