using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
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
        
        private readonly IRestService restService;
        private readonly DesignerUserCredentials designerUserCredentials;

        public DesignerQuestionnairesApiController(
            ICommandService commandService, 
            ILogger logger, 
            IRestService restService,
            DesignerUserCredentials designerUserCredentials)
            : base(commandService, logger)
        {
            this.restService = restService;
            this.designerUserCredentials = designerUserCredentials;
        }

        [HttpPost]
        [CamelCase]
        public async Task<DataTableResponse<QuestionnaireToBeImported>> QuestionnairesList([FromBody] DataTableRequest request)
        {
            try
            {
                var list = await this.restService.GetAsync<PagedQuestionnaireCommunicationPackage>(
                    url: $@"{this.apiPrefix}/{this.apiVersion}/questionnaires",
                    credentials: this.designerUserCredentials.Get(),
                    queryString: new
                    {
                        PageIndex = request.PageIndex,
                        PageSize = request.PageSize,
                        SortOrder = request.GetSortOrder(),
                        Filter = request.Search.Value
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
                        LastModified = x.LastModifiedDate,
                        CreatedBy = x.OwnerName ?? ""
                    })
                };
            }
            catch (RestException e)
            {
                switch (e.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.Forbidden:
                        this.designerUserCredentials.Set(null);
                        throw new HttpResponseException(HttpStatusCode.Unauthorized);
                }

                Logger.Warn("General Rest Exception on Designer communication", e);
                throw;
            }
            catch (Exception exc)
            {
                Logger.Warn("Exception on Designer communication", exc);
                throw;
            }

        }
    }
}
