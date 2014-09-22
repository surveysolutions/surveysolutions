using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Headquarters.API.Formatters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Resources
{
    [TokenValidationAuthorization]
    [RoutePrefix("api/resources/interviews/v1")]
    [HeadquarterFeatureOnly]
    public class InterviewsResourceController : ApiController
    {
        private readonly IReadSideRepositoryReader<ViewWithSequence<InterviewData>> interviewDataReader;
        private readonly IReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IInterviewSynchronizationDtoFactory factory;

        public InterviewsResourceController(IReadSideRepositoryReader<ViewWithSequence<InterviewData>> interviewDataReader,
            IInterviewSynchronizationDtoFactory factory, IReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.interviewDataReader = interviewDataReader;
            this.factory = factory;
            this.interviewSummaryReader = interviewSummaryReader;
        }

        [Route("{id}", Name = "api.interviewDetails")]
        public HttpResponseMessage Get(string id)
        {
            var interviewData = this.interviewDataReader.GetById(id);
            var interviewSummary = this.interviewSummaryReader.GetById(id);
            if (interviewData == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Interview with id {0} was not found", id));
            }

            string comments = null;
            if (interviewSummary != null && interviewSummary.CommentedStatusesHistory != null &&
                interviewSummary.CommentedStatusesHistory.Any())
            {
                comments = interviewSummary.CommentedStatusesHistory.Last().Comment;
            }

            InterviewData document = interviewData.Document;
            InterviewSynchronizationDto interviewSynchronizationDto = this.factory.BuildFrom(document, comments);

            var result = this.Request.CreateResponse(HttpStatusCode.OK, interviewSynchronizationDto,
                new JsonNetFormatter(new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                }));

            return result;
        }
    }
}