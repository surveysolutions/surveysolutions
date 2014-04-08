﻿using System;
using System.Web.Http;
using Main.Core.View;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.UI.Headquarters.Models.API;

namespace WB.UI.Headquarters.API
{
    [Authorize (Roles = "Headquarter, Supervisor")]
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
 
        [HttpGet]
        [Route("apis/v1/interviews")] //?{templateId}&{templateVersion}&{status}&{interviewerId}&{limit=10}&{offset=1}
        public InterviewApiView InterviewsFiltered(Guid? templateId = null, long? templateVersion = null, 
            InterviewStatus? status = null, Guid? interviewerId = null, int limit = 10, int offset = 1)
        {
            var input = new AllInterviewsInputModel
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
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
            var inputModel = new InterviewDetailsInputModel()
            {
                CompleteQuestionnaireId = id
            };

            var interview = this.interviewDetailsViewFactory.Load(inputModel);

            return new InterviewApiDetails(interview);
        }
    }
}