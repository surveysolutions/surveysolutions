﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API
{
    [CamelCase]
    [RoutePrefix("api/Assignments")]
    public class AssignmentsApiController : ApiController
    {
        private readonly IAssignmentViewFactory assignmentViewFactory;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IPlainStorageAccessor<Assignment> assignmentsStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public AssignmentsApiController(IAssignmentViewFactory assignmentViewFactory,
            IAuthorizedUser authorizedUser,
            IPlainStorageAccessor<Assignment> assignmentsStorage,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.assignmentViewFactory = assignmentViewFactory;
            this.authorizedUser = authorizedUser;
            this.assignmentsStorage = assignmentsStorage;
            this.questionnaireStorage = questionnaireStorage;
        }
        
        [Route("")]
        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public IHttpActionResult Get([FromUri]AssignmentsDataTableRequest request)
        {
            QuestionnaireIdentity questionnaireIdentity = null;
            if (!string.IsNullOrEmpty(request.QuestionnaireId))
            {
                QuestionnaireIdentity.TryParse(request.QuestionnaireId, out questionnaireIdentity);
            }

            var input = new AssignmentsInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Order = request.GetSortOrder(),
                SearchBy = request.Search.Value,
                QuestionnaireId = questionnaireIdentity?.QuestionnaireId,
                QuestionnaireVersion = questionnaireIdentity?.Version,
                ResponsibleId = request.ResponsibleId,
                ShowArchive = request.ShowArchive
            };

            if (this.authorizedUser.IsSupervisor)
            {
                input.SupervisorId = this.authorizedUser.Id;
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

        
        [Route("")]
        [HttpDelete]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowedApi]
        public IHttpActionResult Delete([FromBody]int[] ids)
        {
            if (ids == null) return this.BadRequest();

            foreach (var id in ids)
            {
                Assignment assignment = this.assignmentsStorage.GetById(id);
                assignment.Archive();
            }

            return this.Ok();
        }

        [Route("Unarchive")]
        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowedApi]
        public IHttpActionResult Unarchive([FromBody]int[] ids)
        {
            if (ids == null) return this.BadRequest();
            
            foreach (var id in ids)
            {
                Assignment assignment = this.assignmentsStorage.GetById(id);
                assignment.Unarchive();
            }

            return this.Ok();
        }

        [HttpPost]
        [Route("Assign")]
        [ObserverNotAllowedApi]
        public IHttpActionResult Assign([FromBody] AssignRequest request)
        {
            if (request?.Ids == null) return this.BadRequest();
            foreach (var idToAssign in request.Ids)
            {
                Assignment assignment = this.assignmentsStorage.GetById(idToAssign);
                assignment.Reassign(request.ResponsibleId);
            }

            return this.Ok();
        }

        [HttpPatch]
        [Route("{id:int}/SetQuantity")]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowedApi]
        public IHttpActionResult SetQuantity(int id, [FromBody] UpdateAssignmentRequest request)
        {
            var assignment = this.assignmentsStorage.GetById(id);
            assignment.UpdateQuantity(request.Quantity);
            return this.Ok();
        }

        [HttpPost]
        [Route("Create")]
        [ObserverNotAllowedApi]
        public IHttpActionResult Create([FromBody] CreateAssignmentRequest request)
        {
            if (!this.authorizedUser.IsAdministrator && !this.authorizedUser.IsHeadquarter)
                return this.StatusCode(HttpStatusCode.Forbidden);

            if (request == null)
                return this.BadRequest();

            var questionnaireIdentity =
                new QuestionnaireIdentity(request.QuestionnaireId, request.QuestionnaireVersion);

            var assignment = new Assignment(questionnaireIdentity,
                request.SupervisorId, request.Capacity);

            var untypedQuestionAnswers =
                JsonConvert.DeserializeObject<List<UntypedQuestionAnswer>>(request.AnswersToFeaturedQuestions);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            var identifyingAnswers = untypedQuestionAnswers
                .Select(CommandTransformator.ParseQuestionAnswer)
                .Select(answer => IdentifyingAnswer.Create(assignment, questionnaire, answer.Value.ToString(), answer.Key))
                .ToArray();
            assignment.SetAnswers(identifyingAnswers);
            
            this.assignmentsStorage.Store(assignment, Guid.NewGuid());

            return this.Ok(new {});
        }

        public class CreateAssignmentRequest
        {
            public Guid QuestionnaireId { get; set; }
            public long QuestionnaireVersion { get; set; }
            public Guid SupervisorId { get; set; }
            public string AnswersToFeaturedQuestions { get; set; }
            public int? Capacity { get; set; }
        }

        public class UpdateAssignmentRequest
        {
            public int? Quantity { get; set; }
        }

        public class AssignRequest
        {
            public Guid ResponsibleId { get; set; }

            public int[] Ids { get; set; }
        }

        public class AssignmetsDataTableResponse : DataTableResponse<AssignmentRow>
        {
        }

        public class AssignmentsDataTableRequest : DataTableRequest
        {
            public string QuestionnaireId { get; set; }
            public Guid? ResponsibleId { get; set; }

            public bool ShowArchive { get; set; }
        }
    }
}