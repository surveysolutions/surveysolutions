using System;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    [CamelCase]
    public class AssignmetsApiController : ApiController
    {
        private readonly IAssignmentViewFactory assignmentViewFactory;

        public AssignmetsApiController(IAssignmentViewFactory assignmentViewFactory)
        {
            this.assignmentViewFactory = assignmentViewFactory;
        }
        
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
                ResponsibleId = request.ResponsibleId
            };

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

        public class AssignmetsDataTableResponse : DataTableResponse<AssignmentWithoutIdentifingData>
        {
        }

        public class AssignmentsDataTableRequest : DataTableRequest
        {
            public string QuestionnaireId { get; set; }
            public Guid? ResponsibleId { get; set; }
        }
    }
}