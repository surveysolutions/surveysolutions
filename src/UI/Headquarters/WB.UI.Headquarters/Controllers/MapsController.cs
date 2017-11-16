using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Resources;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
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
using WB.UI.Headquarters.Models.Reports;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class MapsController : BaseController
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IMapRepository mapRepository;
        private readonly IExportFactory exportFactory;

        private readonly IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor;

        private readonly IRecordsAccessorFactory recordsAccessorFactory;

        public MapsController(ICommandService commandService, ILogger logger, 
            IFileSystemAccessor fileSystemAccessor, IMapRepository mapRepository,
            IExportFactory exportFactory, IRecordsAccessorFactory recordsAccessorFactory,
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor) : base(commandService, logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.mapRepository = mapRepository;
            this.exportFactory = exportFactory;
            this.recordsAccessorFactory = recordsAccessorFactory;
            this.mapPlainStorageAccessor = mapPlainStorageAccessor;
        }

        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Maps;
            return View();
        }


        public ActionResult MapList()
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
            };
            return View(model);
        }

        public ActionResult UserMapsLink()
        {
            this.ViewBag.ActivePage = MenuItem.Maps;
            return View();
        }

        [HttpGet]
        public ActionResult Details(string mapName)
        {
            this.ViewBag.ActivePage = MenuItem.Maps;

            if (mapName == null)
                return HttpNotFound();

            MapBrowseItem map = mapPlainStorageAccessor.GetById(mapName);
            if(map == null)
                return HttpNotFound();

            return View(map);
        }

        [HttpDelete]
        public ActionResult DeleteMapUserLink(string mapName, string userName)
        {
            this.mapRepository.DeleteMapUser(mapName, userName);
            return this.Content("ok");
        }

        public ActionResult UserMapsLinking()
        {
            this.ViewBag.ActivePage = MenuItem.Maps;
            var model = new UserMapLinkModel()
            {
                DownloadAllUrl = Url.RouteUrl("DefaultApiWithAction", new {httproute = "", controller = "MapsApi", action = "MappingDownload"}),
                UploadUrl = Url.RouteUrl("Default", new { httproute = "", controller = "Maps", action = "UploadMappings" }),
            };
            return View(model);
        }

        [ActivePage(MenuItem.Maps)]
        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
        public ActionResult MapDetails(string mapName)
        {
            this.ViewBag.ActivePage = MenuItem.Maps;

            if (mapName == null)
                return HttpNotFound();

            MapBrowseItem map = mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                return HttpNotFound();

            return this.View("MapDetails",
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
                MinScale = map.MinScale
            });
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
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> UploadMaps(MapFileUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!this.ModelState.IsValid)
            {
                this.Error(Maps.MapsLoadingError);
                return this.RedirectToAction(nameof(Index));
            }
            
            if (".zip" != this.fileSystemAccessor.GetFileExtension(model.File.FileName).ToLower())
            {
                this.Error(Maps.MapLoadingNotZipError);
                return this.RedirectToAction(nameof(Index));
            }

            string tempStore = null;
            var invalidMaps = new List<string>();
            try
            {
                tempStore = mapRepository.StoreData(model.File.InputStream, model.File.FileName);
                var maps = mapRepository.UnzipAndGetFileList(tempStore);
                
                foreach (var map in maps)
                {
                    try
                    {
                        await mapRepository.SaveOrUpdateMapAsync(map);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error on maps import map {map}", e);
                        invalidMaps.Add(map);
                    }
                }

                if (invalidMaps.Count > 0)
                {
                    this.Error(Maps.MapLoadingInvalidFilesError + string.Join(", ", invalidMaps));
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error on maps import", e);
                this.Error(Maps.MapsLoadingError);

            }
            finally
            {
                if(tempStore!=null)
                    mapRepository.DeleteData(tempStore);
            }

            return this.RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult UploadMappings(MapFileUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction(nameof(Index));
            }

            if (".tsv" != this.fileSystemAccessor.GetFileExtension(model.File.FileName).ToLower())
            {
                this.Error("Error occurred. File is not a .tsv");
                return this.RedirectToAction(nameof(Index));
            }

            bool noErrors = true;
            try
            {
                var mappings = ProcessDataFile(model);
                foreach (var mapUserMapping in mappings)
                {
                    noErrors = noErrors && mapRepository.UpdateUserMaps(mapUserMapping.Map, mapUserMapping.Users.ToArray());
                }

                this.Success("User map imported");
            }
            catch (Exception e)
            {
                Logger.Error($"Error on maps import mapping", e);
                this.Error("Error occurred");
                return this.RedirectToAction("Index");
            }

            if (!noErrors)
                this.Info("Imported with ignores");

            return this.RedirectToAction("Index");
        }

        public class MapUserMapping
        {
            public string Map { set; get; }
            public List<string> Users { set; get; } = new List<string>();
        }


        private List<MapUserMapping> ProcessDataFile(MapFileUploadModel model)
        {
            var records = new List<string[]>();
            try
            {
                var recordsAccessor = this.recordsAccessorFactory.CreateRecordsAccessor(model.File.InputStream, "\t");
                records = recordsAccessor.Records.ToList();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            var dataColumnNamesMappedOnRecordSetter = new Dictionary<string, Action<MapUserMapping, string>>
            {
                {"map", (r, v) => r.Map = v},
                {"users", (r, v) => r.Users = new List<string>(v.Split(','))}
            };
            
            var mappings = new List<MapUserMapping>();

            string[] headerRow = null;
            foreach (string[] record in records)
            {
                if (headerRow == null)
                {
                    headerRow = record.Select(r => r.ToLower()).ToArray();
                    //ThrowIfFileStructureIsInvalid(headerRow, fileName);
                    continue;
                }

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

            return mappings;
        }


        [ObserverNotAllowed]
        public HttpResponseMessage MappingDownload()
        {
            return CreateReportResponse("usermaps");
        }

        private HttpResponseMessage CreateReportResponse(string reportName)
        {
            var exportFile = this.exportFactory.CreateExportFile(ExportFileType.Tab);

            var mapping = mapRepository.GetAllMapUsers();


            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(new string[]{"map", "users"}, mapping));

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(exportFileStream)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(exportFile.MimeType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = $@"{this.fileSystemAccessor.MakeValidFileName(reportName)}{exportFile.FileExtension}"
            };

            return response;
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