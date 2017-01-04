using System;
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
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Models.Template;
using WB.UI.Headquarters.Utils;
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
        [CamelCase]
        public DataTableResponse<QuestionnaireListItemModel> Questionnaires([FromBody] DataTableRequest request)
        {
            var input = new QuestionnaireBrowseInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                Filter = request.Search.Value,
                IsAdminMode = true
            };

            var items = this.questionnaireBrowseViewFactory.Load(input);

            return new DataTableResponse<QuestionnaireListItemModel>
            {
                Draw = request.Draw + 1,
                RecordsTotal = items.TotalCount,
                RecordsFiltered = items.TotalCount,
                Data = items.Items.Select(x => new QuestionnaireListItemModel
                {
                    QuestionnaireId = x.QuestionnaireId,
                    Version = x.Version,
                    Title = x.Title,
                    AllowCensusMode = x.AllowCensusMode,
                    CreationDate = x.CreationDate.FormatDateWithTime(),
                    LastEntryDate = x.LastEntryDate.FormatDateWithTime(),
                    ImportDate = x.ImportDate?.FormatDateWithTime()
                })
            };
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