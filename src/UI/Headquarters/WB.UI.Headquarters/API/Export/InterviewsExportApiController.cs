using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Export
{
    public class InterviewsExportApiController : ApiController
    {
        private readonly IInterviewsToExportViewFactory viewFactory;
        private readonly IInterviewFactory interviewFactory;

        public InterviewsExportApiController(IInterviewsToExportViewFactory viewFactory,
            IInterviewFactory interviewFactory)
        {
            this.viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
            this.interviewFactory = interviewFactory ?? throw new ArgumentNullException(nameof(interviewFactory));
        }

        [Route("api/export/v1/interview")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage Get(string questionnaireIdentity,
            InterviewStatus? status, 
            DateTime? fromDate,
            DateTime? toDate)
        {
            var result = viewFactory.GetInterviewsToExport(QuestionnaireIdentity.Parse(questionnaireIdentity), status, fromDate, toDate);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("api/export/v1/interview/{id:guid}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetInterview(Guid id)
        {
            var entities = this.interviewFactory.GetInterviewEntities(id);

            return Request.CreateResponse(HttpStatusCode.OK, entities);
        }
    }
}
