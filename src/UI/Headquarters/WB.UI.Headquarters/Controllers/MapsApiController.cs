using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Resources;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Implementation.Maps;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Shared.Web.Extensions;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.UI.Headquarters.Controllers
{
    [CamelCase]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class MapsApiController : BaseApiController
    {
        private readonly string[] permittedMapFileExtensions = { ".tpk", ".mmpk", ".tif" };

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IMapBrowseViewFactory mapBrowseViewFactory;
        private readonly ILogger logger;
        private readonly IMapStorageService mapStorageService;
        private readonly IExportFactory exportFactory;
        private readonly IMapService mapPropertiesProvider;
        private readonly ICsvReader recordsAccessorFactory;
        private readonly IArchiveUtils archiveUtils;

        public MapsApiController(ICommandService commandService,
            IMapBrowseViewFactory mapBrowseViewFactory, ILogger logger,
            IMapStorageService mapStorageService, IExportFactory exportFactory,
            IMapService mapPropertiesProvider,
            IFileSystemAccessor fileSystemAccessor,
            ICsvReader recordsAccessorFactory,
            IArchiveUtils archiveUtils) : base(commandService, logger)
        {
            this.mapBrowseViewFactory = mapBrowseViewFactory;
            this.logger = logger;
            this.mapStorageService = mapStorageService;
            this.exportFactory = exportFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.mapPropertiesProvider = mapPropertiesProvider;
            this.recordsAccessorFactory = recordsAccessorFactory;
            this.archiveUtils = archiveUtils;
        }

        
        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public IHttpActionResult MapList([FromUri] DataTableRequest request)
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
                    Size = FileSizeUtils.SizeInMegabytes(x.Size)
                })
            };

            return Ok(table);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public IHttpActionResult UserMaps([FromUri] DataTableRequest request)
        {
            var input = new UserMapsInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                SearchBy = request.Search.Value,
            };

            var items = this.mapBrowseViewFactory.Load(input);

            var table = new DataTableResponse<UserMapsViewItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = items.TotalCount,
                RecordsFiltered = items.TotalCount,
                Data = items.Items.ToList().Select(x => new UserMapsViewItem
                {
                    UserName = x.UserName,
                    Maps = x.Maps.ToList()
                })
            };

            return Ok(table);
        }

        public class MapUsersTableRequest : DataTableRequest
        {
            public string MapName { get; set; }
        }


        [HttpPost]
        [ObserverNotAllowedApi]
        public async Task<JsonMapResponse> Upload(HttpFile file)
        {
            var response = new JsonMapResponse();

            if (file == null)
            {
                response.Errors.Add(Maps.MapsLoadingError);
                return response;
            }

            if (".zip" != this.fileSystemAccessor.GetFileExtension(file.FileName).ToLower())
            {
                response.Errors.Add(Maps.MapLoadingNotZipError);
                return response;
            }

            if (!this.mapPropertiesProvider.IsEngineEnabled())
            {
                logger.Error($"Map engine is not initialized");
                response.Errors.Add(Maps.MapEngineIsNotInitialized);
                return response;
            }
            try
            {
                var filesInArchive = archiveUtils.GetArchivedFileNamesAndSize(file.FileBytes);
                
                var validMapFilesCount = filesInArchive.Keys.Count(x =>
                    permittedMapFileExtensions.Contains(this.fileSystemAccessor.GetFileExtension(x)));

                if (validMapFilesCount == 0)
                {
                    response.Errors.Add(Maps.MapLoadingNoMapsInArchive);
                    return response;
                }
                if (filesInArchive.Count > validMapFilesCount)
                {
                    response.Errors.Add(Maps.MapLoadingUnsupportedFilesInArchive);
                    return response;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Invalid map archive", ex);

                response.Errors.Add(Maps.MapsLoadingError);
                return response;
            }

            var invalidMaps = new List<string>();
            try
            {
                var extractedFiles = archiveUtils.GetFilesFromArchive(file.FileBytes);
                foreach (var map in extractedFiles)
                {
                    try
                    {
                        await mapStorageService.SaveOrUpdateMapAsync(map);
                    }

                    catch (Exception e)
                    {
                        logger.Error($"Error on maps import map {map.Name}", e);
                        invalidMaps.Add(map.Name);
                    }
                }

                if (invalidMaps.Count > 0)
                    response.Errors.AddRange(invalidMaps.Select(x => String.Format(Maps.MapLoadingInvalidFile, x)).ToList());
                else
                    response.IsSuccess = true;
            }
            catch (Exception e)
            {
                logger.Error("Error on maps import", e);
                response.Errors.Add(Maps.MapsLoadingError);
                return response;
            }

            return response;
        }

        [HttpPost]
        [ObserverNotAllowed]
        public HttpResponseMessage UploadMappings(HttpFile file)
        {
            if (file == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable, Maps.MappingsLoadingError);
            }

            if (TabExportFile.Extention != this.fileSystemAccessor.GetFileExtension(file.FileName).ToLower())
            {
                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable, Maps.FileLoadingNotTsvError);
            }

            int errorsCount = 0;

            List<MapUserMapping> mappings;

            try
            {
                mappings = ProcessDataFile(file.FileBytes);
            }
            catch (Exception e)
            {
                Logger.Error($"Error on maps import mapping", e);

                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable, Maps.MappingsLoadingError);
            }

            foreach (var mapUserMapping in mappings)
            {
                try
                {
                    mapStorageService.UpdateUserMaps(mapUserMapping.Map, mapUserMapping.Users.ToArray());
                }
                catch
                {

                    errorsCount++;
                }

            }

            return this.Request.CreateResponse(HttpStatusCode.OK, string.Format(Maps.UploadMappingsSummaryFormat, mappings.Count, errorsCount));
        }

        [HttpPost]
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


        
        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor, Interviewer")]
        public IHttpActionResult MapUserList([FromUri]MapUsersTableRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MapName))
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

        
        [HttpGet]
        public HttpResponseMessage MappingDownload()
        {
            return CreateMapUsersResponse("usermaps");
        }

        private HttpResponseMessage CreateMapUsersResponse(string reportName)
        {
            var exportFile = this.exportFactory.CreateExportFile(ExportFileType.Tab);

            var reportView = mapStorageService.GetAllMapUsersReportView();
            
            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(reportView));

            var result = new ProgressiveDownload(this.Request).ResultMessage(exportFileStream, exportFile.MimeType);

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileNameStar = $@"{this.fileSystemAccessor.MakeValidFileName(reportName)}{exportFile.FileExtension}"
            };
            return result;
        }

        [ObserverNotAllowedApi]
        [HttpDelete]
        [Authorize(Roles = "Administrator, Headquarter")]
        public JsonCommandResponse DeleteMap(DeleteMapRequestModel request)
        {
            this.mapStorageService.DeleteMap(request.Map);
            return new JsonCommandResponse() { IsSuccess = true };
        }

        public class DeleteMapRequestModel
        {
            public string Map { get; set; }
        }

        [ObserverNotAllowedApi]
        [HttpDelete]
        [Authorize(Roles = "Administrator, Headquarter")]
        public JsonCommandResponse DeleteMapUser(DeleteMapUserRequestModel request)
        {
            this.mapStorageService.DeleteMapUserLink(request.Map, request.User);
            return new JsonCommandResponse() { IsSuccess = true };
        }

        public class DeleteMapUserRequestModel
        {
            public string User { get; set; }
            public string Map { get; set; }
        }

        public class MapUserMapping
        {
            public string Map { set; get; }
            public List<string> Users { set; get; } = new List<string>();
        }


        private List<MapUserMapping> ProcessDataFile(byte[] file)
        {
            var records = new List<string[]>();
            try
            {
                using (MemoryStream stream = new MemoryStream(file))
                {
                    records = this.recordsAccessorFactory.ReadRowsWithHeader(stream, "\t").ToList();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error on mapping file processing", e);
            }

            var dataColumnNamesMappedOnRecordSetter = new Dictionary<string, Action<MapUserMapping, string>>
            {
                {"map", (r, v) => r.Map = v},
                {"users", (r, v) =>
                    r.Users = new List<string>(
                        string.IsNullOrWhiteSpace(v) ?
                        new string[0] :
                        v.Split(',').Select(x=> x.Trim()).ToArray())}
            };

            var mappings = new List<MapUserMapping>();
            string[] headerRow = records.First().Select(r => r.ToLower()).ToArray();

            for (int j = 1; j < records.Count; j++)
            {
                var record = records[j];

                var dataRecord = new MapUserMapping();
                for (int i = 0; i < headerRow.Length; i++)
                {
                    var columnName = headerRow[i].ToLower();

                    if (!dataColumnNamesMappedOnRecordSetter.ContainsKey(columnName))
                        continue;

                    var recordSetter = dataColumnNamesMappedOnRecordSetter[columnName];

                    var cellValue = (record[i] ?? "").Trim();

                    var propertySetter = recordSetter;

                    propertySetter(dataRecord, cellValue);
                }

                mappings.Add(dataRecord);
            }

            return mappings.GroupBy(p => p.Map, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
        }
    }
}

