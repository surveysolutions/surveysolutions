using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Humanizer;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
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
            get { return GlobalInfo.GetDesignerUserCredentials(); }
            set { HttpContext.Current.Session[GlobalInfo.GetCurrentUser().Name] = value; }
        }

        private readonly IRestService restService;

        public DesignerQuestionnairesApiController(
            ICommandService commandService, 
            IGlobalInfoProvider globalInfo, 
            ILogger logger, 
            IRestService restService)
            : base(commandService, globalInfo, logger)
        {
            this.restService = restService;
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
    }
}