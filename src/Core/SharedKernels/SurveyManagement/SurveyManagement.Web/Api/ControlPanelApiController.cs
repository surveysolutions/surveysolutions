using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.Synchronization;
using WB.UI.Headquarters.Models;
using WB.UI.Shared.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelApiController : ApiController
    {
        private readonly IReadSideAdministrationService readSideAdministrationService;
        private readonly IIncomingPackagesQueue incomingPackagesQueue;

        public ControlPanelApiController(IReadSideAdministrationService readSideAdministrationService, IIncomingPackagesQueue incomingPackagesQueue)
        {
            this.readSideAdministrationService = readSideAdministrationService;
            this.incomingPackagesQueue = incomingPackagesQueue;
        }

        public InterviewDetailsSchedulerViewModel InterviewDetails()
        {
            return new InterviewDetailsSchedulerViewModel()
            {
                Messages = new string[0]
            };
        }

        public int GetIncomingPackagesQueueLength()
        {
            return this.incomingPackagesQueue.QueueLength;
        }

        public IEnumerable<ReadSideEventHandlerDescription> GetAllAvailableHandlers()
        {
            return this.readSideAdministrationService.GetAllAvailableHandlers();
        }

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
        public void StopReadSideRebuilding()
        {
            this.readSideAdministrationService.StopAllViewsRebuilding();
        }
    }
}