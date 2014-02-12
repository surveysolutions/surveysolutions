using System;
using System.Web.Http;
using Main.Core.View;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using Web.Supervisor.Models.API;

namespace Web.Supervisor.API
{
    [RoutePrefix("apis/v1/questionnaires")]
    [Authorize/*(Roles = "Headquarter")*/]
    public class QuestionnairesController : BaseApiServiceController
    {
        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory;
        private readonly IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory;


        public QuestionnairesController(ILogger logger,
            IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory,
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory)
            :base(logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.questionnaireBrowseItemFactory = questionnaireBrowseItemFactory;
        }

        [Route("")]
        public QuestionnaireApiView Get(int limit = 10, int offset = 1)
        {
            if (limit < 0 || offset < 0)
                return null; //add error responses

            var safeLimit = Math.Min(limit, MaxPageSize); //move validation to upper level

            var questionnairesFromStore = this.questionnaireBrowseViewFactory.Load(
                new QuestionnaireBrowseInputModel()
                {
                    PageSize = safeLimit,
                    Page = offset
                });

            return new QuestionnaireApiView(questionnairesFromStore);
        }

        [Route("{id:guid}/{version:int?}")]
        [HttpGet]
        public QuestionnaireApiView Questionnaire(Guid id, int? version, int limit = 10, int offset = 1)
        {
            if (limit < 0 || offset < 0)
                return null; //add error responses

            var safeLimit = Math.Min(limit, MaxPageSize); //move validation to upper level

            var questionnaires = this.questionnaireBrowseViewFactory.Load(
                new QuestionnaireBrowseInputModel()
                {
                    PageSize = safeLimit,
                    Page = offset,
                    QuestionnaireId = id,
                    Version = version
                });

            return new QuestionnaireApiView(questionnaires);
        }
    }
}