using System;
using System.Web.Http;
using Core.Supervisor.Views.Interview;
using Main.Core.View;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.API
{
    //[RoutePrefix("api/v1/interviews")]
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
        
        public AllInterviewsView Get(int limit, int offset)
        {
            var input = new AllInterviewsInputModel
            {
                Page = offset,
                PageSize = limit
            };

            return this.allInterviewsViewFactory.Load(input);
        }

        public InterviewDetailsView Details(Guid id)
        {
            InterviewDetailsInputModel inputModel = new InterviewDetailsInputModel()
            {
                CompleteQuestionnaireId = id
            };

            return interviewDetailsViewFactory.Load(inputModel);
        }
    }
}