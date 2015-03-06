using System;
using System.Collections.Generic;
using System.Web.Http;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [RoutePrefix("apis/v1/questionnaires")]
    [Authorize(Roles = "Headquarter, Supervisor")]
    public class QuestionnairesController : BaseApiServiceController
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory;


        public QuestionnairesController(ILogger logger,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory)
            :base(logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.questionnaireBrowseItemFactory = questionnaireBrowseItemFactory;
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
    }
}