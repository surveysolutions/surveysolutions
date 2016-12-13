using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Humanizer;
using Microsoft.Practices.ServiceLocation;
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

        public class TableInfo
        {
            public class SortOrder
            {
               public int Column { get; set; }
               public OrderDirection Dir { get; set; }
            }

            public class SearchInfo
            {
                public string Value { get; set; }
                public bool Regex { get; set; }
            }

            public class ColumnInfo
            {
                public int Title { get; set; }
                public string Data { get; set; }
                public string Name { get; set; }
                public bool Searchable { get; set; }
                public bool Orderable { get; set; }
                public SearchInfo Search { get; set; }
            }
            public int Draw { get; set; }
            public int Start { get; set; }
            public int Length { get; set; }
            public List<SortOrder> Order { get; set; }
            public List<ColumnInfo> Columns { get; set; }
            public SearchInfo Search { get; set; }
            public int PageIndex => 1 + this.Start/this.Length;
            public int PageSize => this.Length;

            public string GetSortOrder()
            {
                var order = Order.FirstOrDefault();
                if (order == null)
                    return string.Empty;

                var columnName = this.Columns[order.Column].Name;
                var stringifiedOrder = order.Dir == OrderDirection.Asc ? string.Empty : OrderDirection.Desc.ToString();

                return $"{columnName} {stringifiedOrder}";
            }
        }

        [HttpPost]
        [CamelCase]
        public async Task<object> QuestionnairesListNew([FromBody] TableInfo info)
        {
            var list = await this.restService.GetAsync<PagedQuestionnaireCommunicationPackage>(
                url: $"{this.apiPrefix}/{this.apiVersion}/questionnaires",
                credentials: this.designerUserCredentials,
                queryString: new
                {
                    Filter = info.Search.Value,
                    PageIndex = info.PageIndex,
                    PageSize = info.PageSize,
                    SortOrder = info.GetSortOrder()
                });

            return new 
            {
                Draw = info.Draw + 1,
                RecordsTotal = list.TotalCount,
                RecordsFiltered = list.TotalCount,
                Data = list.Items.Select(x => new {
                    Id = x.Id,
                    Title = x.Title,
                    LastModified = new {
                        Display = HumanizeLastUpdateDate(x.LastModifiedDate),
                        Timestamp= x.LastModifiedDate?.Ticks ?? 0
                    },
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