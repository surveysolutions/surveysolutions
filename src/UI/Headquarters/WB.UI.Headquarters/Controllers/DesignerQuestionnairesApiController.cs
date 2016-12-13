using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Humanizer;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Template;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [ApiValidationAntiForgeryToken]
    public class DesignerQuestionnairesApiController : BaseApiController
    {
        private readonly string apiPrefix = @"/api/hq";
        private readonly string apiVersion = @"v3";

        internal RestCredentials designerUserCredentials
        {
            get { return this.getDesignerUserCredentials(this.GlobalInfo); }
            set { SetDesignerUserCredentials(this.GlobalInfo, value); }
        }

        private readonly IRestService restService;
        private readonly IQuestionnaireImportService importService;
        private readonly Func<IGlobalInfoProvider, RestCredentials> getDesignerUserCredentials;

        public DesignerQuestionnairesApiController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger, IRestService restService, IQuestionnaireImportService importService)
            : this(commandService, globalInfo, logger, GetDesignerUserCredentials, restService, importService)
        {
            
        }

        internal DesignerQuestionnairesApiController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            Func<IGlobalInfoProvider, RestCredentials> getDesignerUserCredentials, IRestService restService, IQuestionnaireImportService importService)
            : base(commandService, globalInfo, logger)
        {
            this.getDesignerUserCredentials = getDesignerUserCredentials;
            this.restService = restService;
            this.importService = importService;
        }

        private static RestCredentials GetDesignerUserCredentials(IGlobalInfoProvider globalInfoProvider)
        {
            return globalInfoProvider.GetDesignerUserCredentials();
        }

        private static void SetDesignerUserCredentials(IGlobalInfoProvider globalInfoProvider, RestCredentials designerUserCredentials)
        {
            HttpContext.Current.Session[globalInfoProvider.GetCurrentUser().Name] = designerUserCredentials;
        }

        [HttpPost]
        [CamelCase]
        public async Task<DataTableResponse<QuestionnaireToBeImported>> QuestionnairesListNew([FromBody] DataTableRequest request)
        {
            var list = await this.restService.GetAsync<PagedQuestionnaireCommunicationPackage>(
                url: $"{this.apiPrefix}/{this.apiVersion}/questionnaires",
                credentials: this.designerUserCredentials,
                queryString: new
                {
                    Filter = request.Search.Value,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    SortOrder = request.GetSortOrder()
                });

            return new DataTableResponse<QuestionnaireToBeImported>
            {
                Draw = request.Draw + 1,
                RecordsTotal = list.TotalCount,
                RecordsFiltered = list.TotalCount,
                Data = list.Items.Select(x => new QuestionnaireToBeImported
                {
                    Id = x.Id,
                    Title = x.Title,
                    LastModified = HumanizeLastUpdateDate(x.LastModifiedDate),
                    CreatedBy = x.OwnerName ?? ""
                })
            };
        }

        private string HumanizeLastUpdateDate(DateTime? date)
        {
            if (!date.HasValue) return string.Empty;

            var localDate = date.Value.ToLocalTime();

            var twoDaysAgoAtNoon = DateTime.Now.ToLocalTime().AddDays(-1).AtNoon();

            if (localDate < twoDaysAgoAtNoon)
                // from Designer
                return localDate.ToString("d MMM yyyy, HH:mm");
            
            return localDate.Humanize();
        }

        public async Task<DesignerQuestionnairesView> QuestionnairesList(DesignerQuestionnairesListViewModel data)
        {
            var list = await this.restService.GetAsync<PagedQuestionnaireCommunicationPackage>(
                url: $"{this.apiPrefix}/{this.apiVersion}/questionnaires",
                credentials: this.designerUserCredentials,
                queryString: new
                {
                    Filter = data.Filter,
                    PageIndex = data.PageIndex,
                    PageSize = data.PageSize,
                    SortOrder = data.SortOrder.GetOrderRequestString()
                });

            return new DesignerQuestionnairesView()
                {
                    Items = list.Items.Select(x => new DesignerQuestionnaireListViewItem() { Id = x.Id, Title = x.Title }),
                    TotalCount = list.TotalCount
                };
        }

        [HttpPost]
        [Obsolete("Delete when KP-8251 ")]
        public async Task<QuestionnaireImportResult> GetQuestionnaire(ImportQuestionnaireRequest request)
        {
            return 
                await
                    this.importService.Import(request.Questionnaire.Id, request.Questionnaire.Title, request.AllowCensusMode);
        }
    }
}