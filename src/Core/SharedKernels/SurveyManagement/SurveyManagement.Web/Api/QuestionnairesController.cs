using System;
using System.Collections.Generic;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [RoutePrefix("api/v1/questionnaires")]
    [ApiBasicAuth(new[] { UserRoles.ApiUser }, TreatPasswordAsPlain = true)]
    public class QuestionnairesController : BaseApiServiceController
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory;
        private readonly IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory;

        public QuestionnairesController(ILogger logger,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory,
            IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory)
            :base(logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.questionnaireBrowseItemFactory = questionnaireBrowseItemFactory;
            this.allInterviewsViewFactory = allInterviewsViewFactory;
        }

        [HttpGet]
        [Route("")]
        public QuestionnaireApiView Questionnaires(int limit = 10, int offset = 1)
        {
            var input = new QuestionnaireBrowseInputModel()
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
            };

            var questionnairesFromStore = this.questionnaireBrowseViewFactory.Load(input);

            return new QuestionnaireApiView(questionnairesFromStore);
        }

        [HttpGet]
        [Route("{id:guid}/{version:long?}")]
        public QuestionnaireApiView Questionnaires(Guid id, long? version = null, int limit = 10, int offset = 1)
        {
            var input = new QuestionnaireBrowseInputModel()
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
                QuestionnaireId = id,
                Version = version
            };

            var questionnaires = this.questionnaireBrowseViewFactory.Load(input);

            return new QuestionnaireApiView(questionnaires);
        }
        
        [HttpGet]
        [Route("statuses")]
        public IEnumerable<string> QuestionnairesStatuses()
        {
            return Enum.GetNames(typeof(InterviewStatus));
        }

        [HttpGet]
        [Route("{id:guid}/{version:long}/interviews")]
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
    }
}