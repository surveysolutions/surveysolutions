using AutoMapper;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.PublicApi
{
    [ApiBasicAuth(UserRoles.ApiUser, TreatPasswordAsPlain = true)]
    [RoutePrefix("api/v1/assignments")]
    public class AssignmentsController : BaseApiServiceController
    {
        private readonly IPlainStorageAccessor<Assignment> assignmentsStorage;
        private readonly IAssignmentViewFactory assignmentViewFactory;
        private readonly IMapper mapper;
        private readonly HqUserManager userManager;
        private readonly IPreloadedDataVerifier preloadedDataVerifier;

        public AssignmentsController(
            IAssignmentViewFactory assignmentViewFactory,
            IPlainStorageAccessor<Assignment> assignmentsStorage,
            IPreloadedDataVerifier preloadedDataVerifier,
            IMapper mapper,
            HqUserManager userManager,
            ILogger logger) : base(logger)
        {
            this.assignmentViewFactory = assignmentViewFactory;
            this.assignmentsStorage = assignmentsStorage;
            this.mapper = mapper;
            this.userManager = userManager;
            this.preloadedDataVerifier = preloadedDataVerifier;
        }

        /// <summary>
        /// Get Assignment details
        /// </summary>
        /// <returns>Details of Assignment</returns>
        [HttpGet]
        [Route("{id:int}")]
        public AssignmentDetails Details(int id)
        {
            var assignment = assignmentsStorage.GetById(id);
            return this.mapper.Map<AssignmentDetails>(assignment);
        }

        [HttpGet]
        [Route("")]
        public AssignmentsListView List([FromUri(SuppressPrefixCheck = true, Name = "")] AssignmentsListFilter filter)
        {
            filter = filter ?? new AssignmentsListFilter
            {
                Page = 1,
                PageSize = 20
            };

            filter.PageSize = filter.PageSize == 0 ? 20 : Math.Min(filter.PageSize, 100);

            QuestionnaireIdentity questionnaireId;
            if (!QuestionnaireIdentity.TryParse(filter.QuestionnaireId, out questionnaireId))
            {
                questionnaireId = null;
            }

            AssignmentsWithoutIdentifingData result = this.assignmentViewFactory.Load(new AssignmentsInputModel
            {
                QuestionnaireId = questionnaireId?.QuestionnaireId,
                QuestionnaireVersion = questionnaireId?.Version,
                ResponsibleId = this.GetResponsibleIdPersonFromRequestValue(filter.Responsible),
                Order = filter.Order,
                Page = Math.Max(filter.Page, 1),
                PageSize = filter.PageSize,
                SearchBy = filter.SearchBy,
                ShowArchive = filter.ShowArchive,
                SupervisorId = filter.SupervisorId
            });

            var listView = new AssignmentsListView(result.Page, result.PageSize, result.TotalCount, filter.Order);

            listView.Assignments = this.mapper.Map<List<AssignmentViewItem>>(result.Items);
            return listView;
        }

        [HttpPost]
        [Route]
        public HttpResponseMessage Create(CreateAssignmentRequest createItem)
        {
            var responsible = this.GetResponsibleIdPersonFromRequestValue(createItem.Responsible);

            if (responsible == null)
            {
                throw new ArgumentException(nameof(CreateAssignmentRequest.Responsible), "Cannot identify user from argument: " + createItem.Responsible);
            }

            var assignment = new Assignment(QuestionnaireIdentity.Parse(createItem.QuestionnaireId), responsible.Value, createItem.Capacity);

            assignment = this.mapper.Map(createItem.IdentifyingData, assignment);

            var preloadData = this.mapper.Map<PreloadedDataByFile>(assignment);
            var qid = assignment.QuestionnaireId;
            var verifyResult = this.preloadedDataVerifier.VerifyAssignmentsSample(qid.QuestionnaireId, qid.Version, preloadData);
            verifyResult.WasResponsibleProvided = true;

            var statusCode = HttpStatusCode.BadRequest;

            if (!verifyResult.Errors.Any())
            {
                this.assignmentsStorage.Store(assignment, null);
                assignment = this.assignmentsStorage.GetById(assignment.Id);
                statusCode = HttpStatusCode.OK;
            }

            return Request.CreateResponse(statusCode, new CreateAssignmentResult
            {
                Assignment = mapper.Map<AssignmentDetails>(assignment),
                VerificationStatus = verifyResult
            });
        }

        /// <summary>
        /// Assign new responsible person for assignment
        /// </summary>
        /// <param name="id">Assignment id</param>
        /// <param name="responsible">Responsible user id or name</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id:int}/assign")]
        public AssignmentDetails Assign(int id, [FromBody] AssignmentAssignRequest responsible)
        {
            var assignment = assignmentsStorage.GetById(id);

            var responsibleUserId = this.GetResponsibleIdPersonFromRequestValue(responsible.Responsible);

            if (responsibleUserId == null)
            {
                throw new ArgumentException(nameof(AssignmentAssignRequest.Responsible), "Cannot identify user from argument: " + responsible.Responsible);
            }

            assignment.Reassign(responsibleUserId.Value);

            assignmentsStorage.Store(assignment, id);

            return this.mapper.Map<AssignmentDetails>(assignmentsStorage.GetById(id));
        }

        private Guid? GetResponsibleIdPersonFromRequestValue(string responsible)
        {
            if (string.IsNullOrWhiteSpace(responsible))
            {
                return null;
            }

            Guid responsibleUserId;

            if (!Guid.TryParse(responsible, out responsibleUserId))
            {
                var user = this.userManager.FindByName(responsible);

                if (user == null)
                {
                    return null;
                }

                responsibleUserId = user.Id;
            }

            return responsibleUserId;
        }

        [HttpPatch]
        [Route("{id:int}/changeCapacity")]
        public AssignmentDetails ChangeCapacity(int id, [FromBody] int? capacity)
        {
            var assignment = assignmentsStorage.GetById(id);

            assignment.UpdateCapacity(capacity);

            assignmentsStorage.Store(assignment, id);

            return this.mapper.Map<AssignmentDetails>(assignmentsStorage.GetById(id));
        }
    }
}