using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class DesignerQuestionnairesApiController : ControllerBase
    {   
        private readonly IDesignerApi designerApi;
        private readonly IDesignerUserCredentials designerUserCredentials;

        public DesignerQuestionnairesApiController(
            IDesignerApi designerApi,
            IDesignerUserCredentials designerUserCredentials)
        {
            this.designerApi = designerApi;
            this.designerUserCredentials = designerUserCredentials;
        }

        [HttpGet]
        public async Task<ActionResult<DataTableResponse<QuestionnaireToBeImported>>> QuestionnairesList(DataTableRequest request)
        {
            try
            {
                var list = await this.designerApi.GetQuestionnairesList(new DesignerQuestionnairesListFilter
                {
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    SortOrder = request.GetSortOrder(),
                    Filter = request.Search?.Value
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
                        return Unauthorized();
                }

                throw;
            }
        }
    }
}
