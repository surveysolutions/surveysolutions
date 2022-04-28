using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
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
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class MapsController : Controller
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUserViewFactory userViewFactory;
        private readonly IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor;

        public MapsController(
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor,
            IAuthorizedUser authorizedUser,
            IUserViewFactory userViewFactory)
        {
            this.mapPlainStorageAccessor = mapPlainStorageAccessor;
            this.authorizedUser = authorizedUser;
            this.userViewFactory = userViewFactory;
        }

        [ActivePage(MenuItem.Maps)]
        public ActionResult Index()
        {
            var model = new MapsModel()
            {
                DataUrl = Url.Action("MapList", "MapsApi"),
                UploadMapsFileUrl = Url.Action("Upload", "MapsApi"),
                UserMapsUrl = Url.Action("UserMaps", "Maps"),
                UserMapLinkingUrl = Url.Action("UserMapsLink", "Maps"),
                DeleteMapLinkUrl = Url.Action("DeleteMap", "MapsApi"),
                IsObserver = authorizedUser.IsObserver,
                IsObserving = authorizedUser.IsObserving,
            };
            return this.View(model);
        }

        [ActivePage(MenuItem.Maps)]
        public ActionResult UserMapsLink()
        {
            var model = new UserMapLinkModel()
            {
                DownloadAllUrl = Url.Action("MappingDownload", "MapsApi"),
                UploadUrl = Url.Action("UploadMappings", "MapsApi"),
                MapsUrl = Url.Action("Index", "Maps"),
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
                DataUrl = Url.Action("UserMaps", "MapsApi"),
                MapsUrl = Url.Action("Index", "Maps"),
                UserMapLinkingUrl = Url.Action("UserMapsLink", "Maps"),
            };
            return View("UserMaps", model);
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

            var uploadedBy = map.UploadedBy.HasValue
                ? userViewFactory.GetUser(map.UploadedBy.Value)?.UserName
                : (string)null;
            return this.View("Details",
                new MapDetailsModel
                {
                    DataUrl = Url.Action("MapUserList", "MapsApi"),
                    MapPreviewUrl = Url.Action("MapPreview", "Maps", new { mapName = map.Id }),
                    MapsUrl = Url.Action("Index", "Maps"),
                    FileName = mapName,
                    Size = FileSizeUtils.SizeInMegabytes(map.Size),
                    Wkid = map.Wkid,
                    ImportDate = map.ImportDate,
                    UploadedBy = uploadedBy,
                    MaxScale = map.MaxScale,
                    MinScale = map.MinScale,
                    ShapeType = map.ShapeType,
                    ShapesCount = map.ShapesCount,
                    DeleteMapUserLinkUrl = Url.Action("DeleteMapUser", "MapsApi"),
                    DuplicateMapLabels = map.DuplicateLabels?.Select(l => new DuplicateLabelModel()
                    {
                        Label = l.Label,
                        Count = l.Count
                    }).ToArray() ?? Array.Empty<DuplicateLabelModel>(),
                    IsPreviewGeoJson = map.IsPreviewGeoJson,
                });
        }

        [HttpGet]
        [ActivePage(MenuItem.Maps)]
        [ExtraHeaderPermissions(HeaderPermissionType.Esri)]
        public ActionResult MapPreview(string mapName)
        {
            MapBrowseItem map = mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                return NotFound();

            return View(map);
        }
        
        [HttpGet]
        [ActivePage(MenuItem.Maps)]
        [ExtraHeaderPermissions(HeaderPermissionType.Esri)]
        public ActionResult MapPreviewJson(string mapName)
        {
            var map = mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                return NotFound();

            return this.Content(map.GeoJson, "application/json");
        }
    }
}
