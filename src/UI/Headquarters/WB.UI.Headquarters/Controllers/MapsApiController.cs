using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [ApiValidationAntiForgeryToken]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class MapsApiController : ApiController
    {
        private readonly IMapBrowseViewFactory mapBrowseViewFactory;
        private readonly ILogger logger;
        private readonly IMapRepository mapRepository;

        public MapsApiController(IMapBrowseViewFactory mapBrowseViewFactory, ILogger logger, IMapRepository mapRepository) 
        {
            this.mapBrowseViewFactory = mapBrowseViewFactory;
            this.logger = logger;
            this.mapRepository = mapRepository;
        }

        [HttpPost]
        [CamelCase]
        [Authorize(Roles = "Administrator, Headquarter")]
        public IHttpActionResult Maps([FromBody] DataTableRequest request)
        {
            var input = new MapsInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                SearchBy = request.Search.Value,
            };

            var items = this.mapBrowseViewFactory.Load(input);

            var table = new DataTableResponse<MapViewItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = items.TotalCount,
                RecordsFiltered = items.TotalCount,
                Data = items.Items.ToList().Select(x => new MapViewItem
                {
                    FileName = x.FileName,
                    ImportDate = x.ImportDate?.FormatDateWithTime(),
                    MaxScale = x.MaxScale,
                    MinScale = x.MinScale,
                    Size = x.Size
                })
            };

            return Ok(table);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        public JsonCommandResponse DeleteMap(DeleteMapRequestModel request)
        {
            this.mapRepository.DeleteMap(request.Map);
            return new JsonCommandResponse() { IsSuccess = true };
        }

        public class DeleteMapRequestModel
        {
            public string Map { get; set; }
        }
    }
}
