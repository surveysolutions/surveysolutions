using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Versions;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api
{
    [NoTransaction]
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelApiController : ApiController
    {
        private readonly IProductVersion productVersion;
        private readonly IProductVersionHistory productVersionHistory;
        private readonly IReadSideAdministrationService readSideAdministrationService;

        public ControlPanelApiController(
            IProductVersion productVersion,
            IProductVersionHistory productVersionHistory,
            IReadSideAdministrationService readSideAdministrationService)
        {
            this.productVersion = productVersion;
            this.productVersionHistory = productVersionHistory;
            this.readSideAdministrationService = readSideAdministrationService;
        }

        [NoTransaction]
        public dynamic GetVersions()
        {
            var readSideStatus = this.readSideAdministrationService.GetRebuildStatus();

            return new
            {
                Product = this.productVersion.ToString(),
                ReadSide_Application = readSideStatus.ReadSideApplicationVersion,
                ReadSide_Database = readSideStatus.ReadSideDatabaseVersion,

                History = this.productVersionHistory.GetHistory().ToDictionary(
                    change => change.UpdateTimeUtc,
                    change => change.ProductVersion)
            };
        }


        public IEnumerable<ReadSideEventHandlerDescription> GetAllAvailableHandlers()
        {
            return this.readSideAdministrationService.GetAllAvailableHandlers();
        }

        [NoTransaction]
        public ReadSideStatus GetReadSideStatus()
        {
            return this.readSideAdministrationService.GetRebuildStatus();
        }

        [HttpPost]
        public void RebuildReadSide(RebuildReadSideInputViewModel model)
        {
            switch (model.RebuildType)
            {
                case RebuildReadSideType.All:
                    this.readSideAdministrationService.RebuildAllViewsAsync(model.NumberOfSkipedEvents);
                    break;
                case RebuildReadSideType.ByHandlers:
                    this.readSideAdministrationService.RebuildViewsAsync(model.ListOfHandlers, model.NumberOfSkipedEvents);
                    break;
                case RebuildReadSideType.ByHandlersAndEventSource:
                    this.readSideAdministrationService.RebuildViewForEventSourcesAsync(model.ListOfHandlers, model.ListOfEventSources);
                    break;
            }
        }

        [HttpPost]
        [NoTransaction]
        public void StopReadSideRebuilding()
        {
            this.readSideAdministrationService.StopAllViewsRebuilding();
        }
    }
}