using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Maps;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor)]
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
                IsSupervisor = authorizedUser.IsSupervisor
            };
            return this.View(model);
        }

        [ActivePage(MenuItem.Maps)]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter)]
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
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter)]
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
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor)]
        public ActionResult Details(string mapName)
        {
            if (mapName == null)
                return NotFound();

            MapBrowseItem map = mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                return NotFound();

            if (authorizedUser.IsSupervisor)
            {
                if(map.Users.All(u => u.UserName != authorizedUser.UserName))
                    return Forbid(); 
            }

            var uploadedBy = map.UploadedBy.HasValue
                ? userViewFactory.GetUser(map.UploadedBy.Value)?.UserName
                : (string)null;
            var model=
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
                };

            model.Api = new MapDetailsModel.ApiEndpoints
            {
                Users = authorizedUser.IsSupervisor
                    ? Url.Action("InterviewersCombobox", "Teams")
                    : Url.Action("ResponsiblesCombobox", "Teams"),
            };

            return this.View("Details", model);
        }

        [HttpGet]
        [ActivePage(MenuItem.Maps)]
        [ExtraHeaderPermissions(HeaderPermissionType.Esri)]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor)]
        public ActionResult MapPreview(string mapName)
        {
            MapBrowseItem map = mapPlainStorageAccessor.GetById(mapName);
            
            if (map == null)
                return NotFound();

            if (authorizedUser.IsSupervisor)
            {
                if(map.Users.All(u => u.UserName != authorizedUser.UserName))
                    return Forbid(); 
            }

            return View(map);
        }
        
        [HttpGet]
        [ActivePage(MenuItem.Maps)]
        [ExtraHeaderPermissions(HeaderPermissionType.Esri)]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter)]
        public ActionResult MapPreviewJson(string mapName)
        {
            var map = mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                return NotFound();

            return this.Content(map.GeoJson, "application/json");
        }
    }
}
