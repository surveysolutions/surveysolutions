using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Resources;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Maps;
using WB.UI.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [AuthorizeOr403(Roles = "Administrator, Headquarter")]
    public class MapsController : BaseController
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IMapStorageService mapRepository;
        private readonly IExportFactory exportFactory;
        private readonly IAuthorizedUser authorizedUser;

        private readonly IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor;

        private readonly IRecordsAccessorFactory recordsAccessorFactory;

        public MapsController(ICommandService commandService, ILogger logger, 
            IFileSystemAccessor fileSystemAccessor, IMapStorageService mapRepository,
            IExportFactory exportFactory, IRecordsAccessorFactory recordsAccessorFactory,
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor, IAuthorizedUser authorizedUser) : base(commandService, logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.mapRepository = mapRepository;
            this.exportFactory = exportFactory;
            this.recordsAccessorFactory = recordsAccessorFactory;
            this.mapPlainStorageAccessor = mapPlainStorageAccessor;
            this.authorizedUser = authorizedUser;
        }

        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Maps;

            var model = new MapsModel()
            {
                DataUrl = Url.RouteUrl("DefaultApiWithAction",
                    new
                    {
                        httproute = "",
                        controller = "MapsApi",
                        action = "MapList"
                    }),
                
                UploadMapsFileUrl = Url.RouteUrl("Default", new { httproute = "", controller = "Maps", action = "UploadMapsFile" }),
                UserMapLinkingUrl = Url.RouteUrl("Default", new { httproute = "", controller = "Maps", action = "UserMapsLink" }),
                DeleteMapLinkUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "MapsApi", action = "DeleteMap" }),
                IsObserver = authorizedUser.IsObserver,
                IsObserving = authorizedUser.IsObserving,
            };
            return View(model);
        }

        [HttpDelete]
        [ObserverNotAllowed]
        public ActionResult DeleteMap(string mapName)
        {
            this.mapRepository.DeleteMap(mapName);
            return this.Content("ok");
        }

        public ActionResult UserMapsLink()
        {
            this.ViewBag.ActivePage = MenuItem.Maps;
            var model = new UserMapLinkModel()
            {
                DownloadAllUrl = Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "MapsApi", action = "MappingDownload"}),
                UploadUrl = Url.RouteUrl("Default", new { httproute = "", controller = "Maps", action = "UploadMappings" }),
                MapsUrl = Url.RouteUrl("Default", new { httproute = "", controller = "Maps", action = "Index" }),
                IsObserver = authorizedUser.IsObserver,
                IsObserving = authorizedUser.IsObserving,
                FileExtension = TabExportFile.Extention
            };
            return View(model);
        }

        [HttpGet]
        [ActivePage(MenuItem.Maps)]
        public ActionResult Details(string mapName)
        {
            if (mapName == null)
                return HttpNotFound();

            MapBrowseItem map = mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                return HttpNotFound();

            return this.View("Details",
                new MapDetailsModel
                {

                    DataUrl = Url.RouteUrl("DefaultApiWithAction",
                        new
                        {
                            httproute = "",
                            controller = "MapsApi",
                            action = "MapUserList"
                        }),
                    MapPreviewUrl = Url.RouteUrl("Default", new { httproute = "", controller = "Maps", action = "MapPreview", mapName = map.Id }),
                    MapsUrl = Url.RouteUrl("Default", new { httproute = "", controller = "Maps", action = "Index" }),
                    FileName = mapName,
                    Size = FileSizeUtils.SizeInMegabytes(map.Size),
                    Wkid = map.Wkid,
                    ImportDate = map.ImportDate.HasValue ? map.ImportDate.Value.FormatDateWithTime() : "",
                    MaxScale = map.MaxScale,
                    MinScale = map.MinScale,
                    DeleteMapUserLinkUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "MapsApi", action = "DeleteMapUser" })
                });
        }

        [HttpDelete]
        [ObserverNotAllowed]
        public ActionResult DeleteMapUserLink(string mapName, string userName)
        {
            this.mapRepository.DeleteMapUserLink(mapName, userName);
            return this.Content("ok");
        }

                
        [HttpGet]
        public ActionResult MapPreview(string mapName)
        {
            this.ViewBag.ActivePage = MenuItem.Maps;

            MapBrowseItem map = mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                return HttpNotFound();

            return View(map);
        }

        
        [HttpPost]
        [ObserverNotAllowed]
        public async Task<ActionResult> UploadMapsFile(HttpPostedFileBase file)
        {
            if (!this.ModelState.IsValid)
            {
                return this.Content(Maps.MapsLoadingError);
            }

            if (".zip" != this.fileSystemAccessor.GetFileExtension(file.FileName).ToLower())
            {
                return this.Content(Maps.MapLoadingNotZipError);
            }

            string tempStore = null;
            var processedMaps = new Dictionary<string, bool>();

            try
            {
                tempStore = mapRepository.StoreData(file.InputStream, file.FileName);
                var maps = mapRepository.UnzipAndGetFileList(tempStore);

                if (maps == null)
                    this.Content(Maps.MapsLoadingError);

                foreach (var map in maps)
                {
                    try
                    {
                        await mapRepository.SaveOrUpdateMapAsync(map);
                        processedMaps.Add(map, true);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error on maps import map {map}", e);
                        processedMaps.Add(map, false);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error on maps import", e);
                return this.Content(Maps.MapsLoadingError);

            }
            finally
            {
                if (tempStore != null)
                    mapRepository.DeleteTemporaryData(tempStore);
            }

            return this.Content(string.Format(Maps.UploadMapsSummaryFormat, processedMaps.Count, processedMaps.Values.Count(x => x == false)));
        }


        [HttpPost]
        [ObserverNotAllowed]
        public ActionResult UploadMappings(HttpPostedFileBase file)
        {
            if (!this.ModelState.IsValid)
            {
                return this.Content(Maps.MappingsLoadingError);
            }

            if (TabExportFile.Extention != this.fileSystemAccessor.GetFileExtension(file.FileName).ToLower())
            {
                return this.Content(Maps.FileLoadingNotTsvError);
            }

            int errorsCount = 0;

            List<MapUserMapping> mappings;

            try
            {
                mappings = ProcessDataFile(file);
            }
            catch (Exception e)
            {
                Logger.Error($"Error on maps import mapping", e);

                return this.Content(Maps.MappingsLoadingError);
            }
            
             foreach (var mapUserMapping in mappings)
             {
                try
                {
                    mapRepository.UpdateUserMaps(mapUserMapping.Map, mapUserMapping.Users.ToArray());
                }
                catch 
                {

                    errorsCount ++;
                }
                    
             }

            return this.Content(string.Format(Maps.UploadMappingsSummaryFormat, mappings.Count, errorsCount));
        }

        public class MapUserMapping
        {
            public string Map { set; get; }
            public List<string> Users { set; get; } = new List<string>();
        }


        private List<MapUserMapping> ProcessDataFile(HttpPostedFileBase file)
        {
            var records = new List<string[]>();
            try
            {
                var recordsAccessor = this.recordsAccessorFactory.CreateRecordsAccessor(file.InputStream, "\t");
                records = recordsAccessor.Records.ToList();
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

    public class MapFileUploadModel
    {
        [Required(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.BatchUploadModel_Required))]
        [ValidateFile(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.BatchUploadModel_ValidationErrorMessage))]
        [Display(ResourceType = typeof(FieldsAndValidations), Name = nameof(FieldsAndValidations.BatchUploadModel_FileName))]
        public HttpPostedFileBase File { get; set; }
        
        public int ClientTimezoneOffset { get; set; }
    }
}