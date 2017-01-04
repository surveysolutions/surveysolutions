using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
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
                Page = data.PageIndex,
                PageSize = data.PageSize,
                Orders = data.SortOrder ?? new List<OrderRequestItem>(),
                Filter = data.Filter
            };

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