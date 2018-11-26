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
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Models.ComponentModels;
using WB.UI.Headquarters.Models.Template;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
   
    [ApiValidationAntiForgeryToken]
    public class QuestionnairesApiController : BaseApiController
    {
        private readonly IAuthorizedUser authorizedUser;
        private const int DEFAULTPAGESIZE = 12;
        private const string DEFAULTEMPTYQUERY = "";

        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IDeleteQuestionnaireService deleteQuestionnaireService;

        public QuestionnairesApiController(
            ICommandService commandService, IAuthorizedUser authorizedUser, ILogger logger,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IDeleteQuestionnaireService deleteQuestionnaireService)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.deleteQuestionnaireService = deleteQuestionnaireService;
        }

        [HttpPost]
        [CamelCase]
        [Authorize(Roles = "Administrator, Headquarter, Interviewer")]
        public DataTableResponse<QuestionnaireListItemModel> Questionnaires([FromBody] DataTableRequest request)
        {
            var input = new QuestionnaireBrowseInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                SearchFor = request.Search.Value,
                IsAdminMode = true
            };

            var items = this.questionnaireBrowseViewFactory.Load(input);

            return new DataTableResponse<QuestionnaireListItemModel>
            {
                Draw = request.Draw + 1,
                RecordsTotal = items.TotalCount,
                RecordsFiltered = items.TotalCount,
                Data = items.Items.ToList().Select(x => new QuestionnaireListItemModel
                {
                    QuestionnaireId = x.QuestionnaireId,
                    Version = x.Version,
                    Title = x.Title,
                    AllowCensusMode = x.AllowCensusMode,
                    CreationDate = x.CreationDate,
                    LastEntryDate = x.LastEntryDate,
                    ImportDate = x.ImportDate,
                    IsDisabled = x.Disabled
                })
            };
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        public QuestionnaireBrowseView AllQuestionnaires(AllQuestionnairesListViewModel data)
        {
            var input = new QuestionnaireBrowseInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                Orders = data.SortOrder ?? new List<OrderRequestItem>(),
                SearchFor = data.SeachFor
            };

            return this.questionnaireBrowseViewFactory.Load(input);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public JsonCommandResponse DeleteQuestionnaire(DeleteQuestionnaireRequestModel request)
        {
            deleteQuestionnaireService.DisableQuestionnaire(request.QuestionnaireId, request.Version, this.authorizedUser.Id);
            
            return new JsonCommandResponse { IsSuccess = true };
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [CamelCase]
        public ComboboxModel QuestionnairesCombobox(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE, bool censusOnly = false)
        {
            var questionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel
            {
                PageSize = pageSize,
                SearchFor = query,
                IsAdminMode = true,
                OnlyCensus = censusOnly
            });

            return new ComboboxModel(questionnaires.Items.Select(x => new ComboboxOptionModel(x.Id, $"(ver. {x.Version}) {x.Title}")).ToArray(), questionnaires.TotalCount);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [CamelCase]
        public ComboboxModel QuestionnairesComboboxById(string questionnaireIdentity, bool censusOnly = false)
        {
            var identity = QuestionnaireIdentity.Parse(questionnaireIdentity);

            var questionnaires = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel
            {
                QuestionnaireId = identity.QuestionnaireId,
                Version = identity.Version,
                IsAdminMode = true,
                OnlyCensus = censusOnly
            });

            return new ComboboxModel(questionnaires.Items.Select(x => new ComboboxOptionModel(x.Id, $"(ver. {x.Version}) {x.Title}")).ToArray(), questionnaires.TotalCount);
        }
    }
}
