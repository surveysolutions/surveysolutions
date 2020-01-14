﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Maps;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Maps;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class MapsController : Controller
    {
        private readonly IAuthorizedUser authorizedUser;

        private readonly IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor;

        public MapsController(ICommandService commandService, ILogger logger,
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor,
            IMapService mapPropertiesProvider, IAuthorizedUser authorizedUser)
        {
            this.mapPlainStorageAccessor = mapPlainStorageAccessor;
            this.authorizedUser = authorizedUser;
        }

        [ActivePage(MenuItem.Maps)]
        public ActionResult Index()
        {
            var model = new MapsModel()
            {
                DataUrl = Url.RouteUrl("DefaultApiWithAction",
                    new
                    {
                        httproute = "",
                        controller = "MapsApi",
                        action = "MapList"
                    }),

                UploadMapsFileUrl = Url.RouteUrl("DefaultApiWithAction",
                    new {httproute = "", controller = "MapsApi", action = "Upload"}),
                UserMapsUrl =
                    Url.RouteUrl("Default", new { httproute = "", controller = "Maps", action = "UserMaps" }),
                UserMapLinkingUrl =
                    Url.RouteUrl("Default", new {httproute = "", controller = "Maps", action = "UserMapsLink"}),
                DeleteMapLinkUrl = Url.RouteUrl("DefaultApiWithAction",
                    new {httproute = "", controller = "MapsApi", action = "DeleteMap"}),
                IsObserver = authorizedUser.IsObserver,
                IsObserving = authorizedUser.IsObserving,
            };
            return this.View(model);
        }

        [ActivePage(MenuItem.Maps)]
        public ActionResult UserMapsLink()
        {
            this.ViewBag.ActivePage = MenuItem.Maps;
            var model = new UserMapLinkModel()
            {
                DownloadAllUrl = Url.RouteUrl("DefaultApiWithAction",
                    new {httproute = "", controller = "MapsApi", action = "MappingDownload"}),
                UploadUrl = Url.RouteUrl("DefaultApiWithAction",
                    new { httproute = "", controller = "MapsApi", action = "UploadMappings" }),
                MapsUrl = Url.RouteUrl("Default", new {httproute = "", controller = "Maps", action = "Index"}),
                IsObserver = authorizedUser.IsObserver,
                IsObserving = authorizedUser.IsObserving,
                FileExtension = TabExportFile.Extention,

                UserMapsUrl = Url.RouteUrl("Default", new { httproute = "", controller = "Maps", action = "UserMaps" })
            };
            return View(model);
        }

        [HttpGet]
        [ActivePage(MenuItem.Maps)]
        public ActionResult UserMaps()
        {
            var model = new UserMapModel()
            {
                DataUrl = Url.RouteUrl("DefaultApiWithAction",
                    new
                    {
                        httproute = "",
                        controller = "MapsApi",
                        action = "UserMaps"
                    }),
                MapsUrl = Url.RouteUrl("Default", new { httproute = "", controller = "Maps", action = "Index" }),
                UserMapLinkingUrl = Url.RouteUrl("Default", new { httproute = "", controller = "Maps", action = "UserMapsLink" })
            };
            return View(model);
        }

        [HttpGet]
        [ActivePage(MenuItem.Maps)]
        public ActionResult Details(string mapName)
        {
            if (mapName == null)
                return NotFound();

            MapBrowseItem map = mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                return NotFound();

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
                    MapPreviewUrl = Url.RouteUrl("Default",
                        new {httproute = "", controller = "Maps", action = "MapPreview", mapName = map.Id}),
                    MapsUrl = Url.RouteUrl("Default", new {httproute = "", controller = "Maps", action = "Index"}),
                    FileName = mapName,
                    Size = FileSizeUtils.SizeInMegabytes(map.Size),
                    Wkid = map.Wkid,
                    ImportDate = map.ImportDate.HasValue ? map.ImportDate.Value.FormatDateWithTime() : "",
                    MaxScale = map.MaxScale,
                    MinScale = map.MinScale,
                    DeleteMapUserLinkUrl = Url.RouteUrl("DefaultApiWithAction",
                        new {httproute = "", controller = "MapsApi", action = "DeleteMapUser"})
                });
        }

        [HttpGet]
        [ActivePage(MenuItem.Maps)]
        public ActionResult MapPreview(string mapName)
        {
            MapBrowseItem map = mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                return NotFound();

            return View(map);
        }
    }
}
