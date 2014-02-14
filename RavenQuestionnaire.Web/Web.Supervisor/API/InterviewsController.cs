using System;
using System.Web.Http;
using Core.Supervisor.Views.Interview;
using Main.Core.View;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using Web.Supervisor.Models.API;

namespace Web.Supervisor.API
{
    
    [Authorize/*(Roles = "Headquarter")*/]
    public class InterviewsController : BaseApiServiceController
    {
        private readonly IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory;
        private readonly IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsViewFactory;

        public InterviewsController(ILogger logger,
            IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory,
            IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsViewFactory)
            : base(logger)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.interviewDetailsViewFactory = interviewDetailsViewFactory;
        }
 
        //would be extended
        [HttpGet]
        [Route("apis/v1/interviews")] //?{templateId}&{templateVersion}&{status}&{interviewerId}&{limit=10}&{offset=1}
        public InterviewApiView InterviewsFiltered(Guid? templateId = null, long? templateVersion = null, 
            InterviewStatus? status = null, Guid? interviewerId = null, int limit = 10, int offset = 1)
        {
            var input = new AllInterviewsInputModel
            {
                Page = offset,
                PageSize = limit,
                QuestionnaireId = templateId,
                QuestionnaireVersion = templateVersion,
                Status = status,
                InterviewId = interviewerId
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
                Page = offset,
                PageSize = limit,
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
            var inputModel = new InterviewDetailsInputModel()
            {
                CompleteQuestionnaireId = id
            };

            var interview = interviewDetailsViewFactory.Load(inputModel);

            return new InterviewApiDetails(interview);
        }
    }
}