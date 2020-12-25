using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Maps;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter)]
    public class MapDashboard : Controller
    {
        private readonly IAuthorizedUser authorizedUser;

        private readonly IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor;

        public MapDashboard(
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor,
            IAuthorizedUser authorizedUser)
        {
            this.mapPlainStorageAccessor = mapPlainStorageAccessor;
            this.authorizedUser = authorizedUser;
        }

        public class MapDashboardModel
        {
            public string DataUrl { get; set; }
            public string Questionnaires { get; set; }
            public string Responsible { get; set; }
        }

        [ActivePage(MenuItem.Interviews)]
        public ActionResult Index()
        {
            var model = new MapDashboardModel()
            {
                DataUrl = Url.Action("Markers", "MapDashboardApi"),
                Questionnaires = Url.Action("Questionnaires", "MapDashboardApi"),
                Responsible = authorizedUser.IsSupervisor
                    ? Url.Action("InterviewersCombobox", "Teams")
                    : Url.Action("ResponsiblesCombobox", "Teams")
            };
            return this.View(model);
        }
    }
}