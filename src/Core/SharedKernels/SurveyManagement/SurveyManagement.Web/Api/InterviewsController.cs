using System;
using System.Net;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class InterviewsController : BaseApiServiceController
    {
        private readonly IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory;
        private readonly IInterviewDetailsViewFactory interviewDetailsViewFactory;
        private readonly IInterviewHistoryFactory interviewHistoryViewFactory;

        public InterviewsController(ILogger logger,
            IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory,
            IInterviewDetailsViewFactory interviewDetailsViewFactory, IInterviewHistoryFactory interviewHistoryViewFactory)
            : base(logger)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.interviewDetailsViewFactory = interviewDetailsViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
        }

        [HttpGet]
        [Route("apis/v1/interviews")] //?{templateId}&{templateVersion}&{status}&{interviewId}&{limit=10}&{offset=1}
        public InterviewApiView InterviewsFiltered(Guid? templateId = null, long? templateVersion = null,
            InterviewStatus? status = null, Guid? interviewId = null, int limit = 10, int offset = 1)
        {
            var input = new AllInterviewsInputModel
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
                QuestionnaireId = templateId,
                QuestionnaireVersion = templateVersion,
                Status = status,
                InterviewId = interviewId
            };

            var interviews = this.allInterviewsViewFactory.Load(input);

            return new InterviewApiView(interviews);
        }

        [HttpGet]
        [Route("apis/v1/questionnaires/{id:guid}/{version:long}/interviews")]
        public InterviewApiView Interviews(Guid id, long version, int limit = 10, int offset = 1)
        {
            var input = new AllInterviewsInputModel
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
                QuestionnaireId = id,
                QuestionnaireVersion = version
            };

            var interviews = this.allInterviewsViewFactory.Load(input);

            return new InterviewApiView(interviews);
        }

        [HttpGet]
        [Route("apis/v1/interviews/{id:guid}/details")]
        public InterviewApiDetails InterviewDetails(Guid id)
        {
            var interview = this.interviewDetailsViewFactory.GetInterviewDetails(interviewId: id);

            if (interview == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var interviewDetails = new InterviewApiDetails(interview.InterviewDetails);

            return interviewDetails;
        }

        [HttpGet]
        [Route("apis/v1/interviews/{id:guid}/history")]
        public InterviewHistoryView InterviewHistory(Guid id)
        {
            var interview = this.interviewHistoryViewFactory.Load(id);

            if (interview == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return interview;
        }
    }
}