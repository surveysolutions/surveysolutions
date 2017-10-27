using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    
    [Authorize(Roles = "Administrator, Headquarter")]
    public class MapsApiController : ApiController
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IMapBrowseViewFactory mapBrowseViewFactory;
        private readonly ILogger logger;
        private readonly IMapRepository mapRepository;
        private readonly IExportFactory exportFactory;

        public MapsApiController(IMapBrowseViewFactory mapBrowseViewFactory, ILogger logger,
            IMapRepository mapRepository, IExportFactory exportFactory,
            IFileSystemAccessor fileSystemAccessor) 
        {
            this.mapBrowseViewFactory = mapBrowseViewFactory;
            this.logger = logger;
            this.mapRepository = mapRepository;
            this.exportFactory = exportFactory;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        [ApiValidationAntiForgeryToken]
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
                    /*MaxScale = x.MaxScale,
                    MinScale = x.MinScale,*/
                    Size = x.Size
                })
            };

            return Ok(table);
        }

        public class MapUsersTableRequest : DataTableRequest
        {
            public string MapName { get; set; }
        }

        [ApiValidationAntiForgeryToken]
        [HttpPost]
        [CamelCase]
        [Authorize(Roles = "Administrator, Headquarter")]
        public IHttpActionResult MapUsers([FromBody] MapUsersTableRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.MapName))
                return null;

            var input = new MapUsersInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                SearchBy = request.Search.Value,
                MapName = request.MapName
            };

            var items = this.mapBrowseViewFactory.Load(input);

            var table = new DataTableResponse<MapUserViewItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = items.TotalCount,
                RecordsFiltered = items.TotalCount,
                Data = items.Items.ToList().Select(x => new MapUserViewItem
                {
                    UserName = x
                })
            };

            return Ok(table);
        }

        public class MapUserViewItem
        {
            public string UserName { set; get; }
        }

        [ObserverNotAllowedApi]
        [HttpGet]
        public HttpResponseMessage MappingDownload()
        {
            return CreateMapUsersResponse("usermaps");
        }

        private HttpResponseMessage CreateMapUsersResponse(string reportName)
        {
            var exportFile = this.exportFactory.CreateExportFile(ExportFileType.Tab);

            var mapping = mapRepository.GetAllMapUsers();
            
            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(new string[] { "map", "users" }, mapping));

            var result = new ProgressiveDownload(this.Request).ResultMessage(exportFileStream, exportFile.MimeType);

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileNameStar = $@"{this.fileSystemAccessor.MakeValidFileName(reportName)}{exportFile.FileExtension}"
            };
            return result;
        }

        [ApiValidationAntiForgeryToken]
        [ObserverNotAllowedApi]
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

        [ApiValidationAntiForgeryToken]
        [ObserverNotAllowedApi]
        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        public JsonCommandResponse DeleteMapUser(DeleteMapUserRequestModel request)
        {
            this.mapRepository.DeleteMapUser(request.Map, request.User);
            return new JsonCommandResponse() { IsSuccess = true };
        }

        public class DeleteMapUserRequestModel
        {
            public string User { get; set; }
            public string Map { get; set; }
        }
    }
}
