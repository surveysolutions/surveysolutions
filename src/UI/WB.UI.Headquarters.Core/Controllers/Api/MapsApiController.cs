using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Implementation.Maps;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Resources;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [Route("api/[controller]/[action]")]
    public class MapsApiController : ControllerBase
    {
        private readonly string[] permittedMapFileExtensions = { ".tpk", ".mmpk", ".tif" };

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IMapBrowseViewFactory mapBrowseViewFactory;
        private readonly ILogger logger;
        private readonly IMapStorageService mapStorageService;
        private readonly IExportFactory exportFactory;
        private readonly ICsvReader recordsAccessorFactory;
        private readonly IArchiveUtils archiveUtils;

        public MapsApiController(
            IMapBrowseViewFactory mapBrowseViewFactory, ILogger logger,
            IMapStorageService mapStorageService, IExportFactory exportFactory,
            IFileSystemAccessor fileSystemAccessor,
            ICsvReader recordsAccessorFactory,
            IArchiveUtils archiveUtils)
        {
            this.mapBrowseViewFactory = mapBrowseViewFactory;
            this.logger = logger;
            this.mapStorageService = mapStorageService;
            this.exportFactory = exportFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.recordsAccessorFactory = recordsAccessorFactory;
            this.archiveUtils = archiveUtils;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public IActionResult MapList(DataTableRequest request)
        {
            var input = new MapsInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                SearchBy = request.Search?.Value,
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
        public IActionResult UserMaps(DataTableRequest request)
        {
            var input = new UserMapsInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                SearchBy = request.Search?.Value,
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
        [ObservingNotAllowed]
        [RequestSizeLimit(500 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
        public async Task<JsonMapResponse> Upload(IFormFile file)
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

            try
            {
                var filesInArchive = archiveUtils.GetArchivedFileNamesAndSize(file.OpenReadStream());
                
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

            var invalidMaps = new List<Tuple<string, Exception>>();
            try
            {
                var extractedFiles = archiveUtils.GetFilesFromArchive(file.OpenReadStream());
                foreach (var map in extractedFiles)
                {
                    try
                    {
                        await mapStorageService.SaveOrUpdateMapAsync(map);
                    }

                    catch (Exception e)
                    {
                        logger.Error($"Error on maps import map {map.Name}", e);
                        invalidMaps.Add(new Tuple<string, Exception>(map.Name, e));
                    }
                }

                if (invalidMaps.Count > 0)
                    response.Errors.AddRange(invalidMaps.Select(x => String.Format(Maps.MapLoadingInvalidFile, x.Item1, x.Item2.Message)).ToList());
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
        [ObservingNotAllowed]
        public IActionResult UploadMappings(IFormFile file)
        {
            if (file == null)
            {
                return this.StatusCode(StatusCodes.Status406NotAcceptable, Maps.MappingsLoadingError);
            }

            if (TabExportFile.Extention != this.fileSystemAccessor.GetFileExtension(file.FileName).ToLower())
            {
                return this.StatusCode(StatusCodes.Status406NotAcceptable, Maps.FileLoadingNotTsvError);
            }

            int errorsCount = 0;

            List<MapUserMapping> mappings;

            try
            {
                mappings = ProcessDataFile(file.OpenReadStream());
            }
            catch (Exception e)
            {
                logger.Error($"Error on maps import mapping", e);

                return this.StatusCode(StatusCodes.Status406NotAcceptable, Maps.MappingsLoadingError);
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

            var message = string.Format(Maps.UploadMappingsSummaryFormat, mappings.Count, errorsCount);
            return this.Ok(message);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        public IActionResult MapUsers(MapUsersTableRequest request)
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
        public IActionResult MapUserList(MapUsersTableRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MapName))
                return null;

            var input = new MapUsersInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                SearchBy = request.Search?.Value,
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
        public IActionResult MappingDownload()
        {
            return CreateMapUsersResponse("usermaps");
        }

        private IActionResult CreateMapUsersResponse(string reportName)
        {
            var exportFile = this.exportFactory.CreateExportFile(ExportFileType.Tab);

            var reportView = mapStorageService.GetAllMapUsersReportView();
            
            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(reportView));

            var fileNameStar = $@"{this.fileSystemAccessor.MakeValidFileName(reportName)}{exportFile.FileExtension}";
            var result = File(exportFileStream, exportFile.MimeType, fileNameStar);

            return result;
        }

        [ObservingNotAllowed]
        [HttpDelete]
        [Authorize(Roles = "Administrator, Headquarter")]
        public async Task<CommandApiController.JsonCommandResponse> DeleteMap([FromBody] DeleteMapRequestModel request)
        {
            await this.mapStorageService.DeleteMap(request.Map);
            return new CommandApiController.JsonCommandResponse() { IsSuccess = true };
        }

        public class DeleteMapRequestModel
        {
            public string Map { get; set; }
        }

        [ObservingNotAllowed]
        [HttpDelete]
        [Authorize(Roles = "Administrator, Headquarter")]
        public CommandApiController.JsonCommandResponse DeleteMapUser([FromBody] DeleteMapUserRequestModel request)
        {
            this.mapStorageService.DeleteMapUserLink(request.Map, request.User);
            return new CommandApiController.JsonCommandResponse() { IsSuccess = true };
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


        private List<MapUserMapping> ProcessDataFile(Stream fileStream)
        {
            var records = new List<string[]>();
            try
            {
                records = this.recordsAccessorFactory.ReadRowsWithHeader(fileStream, "\t").ToList();
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

