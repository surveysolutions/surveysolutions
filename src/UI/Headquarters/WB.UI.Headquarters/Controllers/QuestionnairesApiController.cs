using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [ApiValidationAntiForgeryToken]
    public class QuestionnairesApiController : BaseApiController
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IDeleteQuestionnaireService deleteQuestionnaireService;

        public QuestionnairesApiController(
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IDeleteQuestionnaireService deleteQuestionnaireService)
            : base(commandService, globalInfo, logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.deleteQuestionnaireService = deleteQuestionnaireService;
        }

        [HttpPost]
        public QuestionnaireBrowseView AllQuestionnaires(AllQuestionnairesListViewModel data)
        {
            var input = new QuestionnaireBrowseInputModel
            {
                Orders = data.SortOrder  == null ? new List<OrderRequestItem>() : data.SortOrder.ToList()
            };
            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            if (data.Request != null)
            {
                input.Filter = data.Request.Filter;
            }

            return this.questionnaireBrowseViewFactory.Load(input);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public JsonCommandResponse DeleteQuestionnaire(DeleteQuestionnaireRequestModel request)
        {
            deleteQuestionnaireService.DeleteQuestionnaire(request.QuestionnaireId, request.Version, this.GlobalInfo.GetCurrentUser().Id);
            
            return new JsonCommandResponse() { IsSuccess = true };
        }
    }
}