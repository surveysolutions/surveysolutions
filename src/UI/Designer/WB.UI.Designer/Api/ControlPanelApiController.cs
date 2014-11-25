using System.Collections.Generic;
using System.Web.Http;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelApiController : ApiController
    {
        private readonly IReadSideAdministrationService readSideAdministrationService;

        public ControlPanelApiController(IReadSideAdministrationService readSideAdministrationService)
        {
            this.readSideAdministrationService = readSideAdministrationService;
        }

        [HttpPost]
        public IEnumerable<EventHandlerDescription> GetAllAvailableHandlers()
        {
            return this.readSideAdministrationService.GetAllAvailableHandlers();
        }

        [HttpPost]
        public RebuildReadSideStatus GetReadSideStatus()
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
        public void StopReadSideRebuilding()
        {
            this.readSideAdministrationService.StopAllViewsRebuilding();
        }
    }
}