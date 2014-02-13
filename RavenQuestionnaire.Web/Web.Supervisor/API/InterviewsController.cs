using System;
using System.Web.Http;
using Core.Supervisor.Views.Interview;
using Main.Core.View;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.API
{
    [RoutePrefix("apis/v1/interviews")]
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

        [Route("")]
        public AllInterviewsView Get(int limit = 10, int offset = 1)
        {
            var input = new AllInterviewsInputModel
            {
                Page = offset,
                PageSize = limit
            };

            return this.allInterviewsViewFactory.Load(input);
        }

        [Route("{id:guid}/details")]
        public InterviewDetailsView Get(Guid id)
        {
            var inputModel = new InterviewDetailsInputModel()
            {
                CompleteQuestionnaireId = id
            };

            return interviewDetailsViewFactory.Load(inputModel);
        }
    }
}