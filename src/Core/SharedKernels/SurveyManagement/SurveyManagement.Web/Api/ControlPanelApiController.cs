using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Http;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.Synchronization;
using WB.UI.Headquarters.Models;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;


namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [NoTransaction]
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelApiController : ApiController
    {
        private readonly IReadSideAdministrationService readSideAdministrationService;
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private readonly MemoryCache cache = MemoryCache.Default;


        public ControlPanelApiController(IReadSideAdministrationService readSideAdministrationService,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue)
        {
            this.readSideAdministrationService = readSideAdministrationService;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
        }

        public InterviewDetailsSchedulerViewModel InterviewDetails()
        {
            return new InterviewDetailsSchedulerViewModel
            {
                Messages = new string[0]
            };
        }

        [NoTransaction]
        public int GetIncomingPackagesQueueLength()
        {
            object cachedLength = this.cache.Get("incomingPackagesQueueLength");
            if (cachedLength != null)
            {
                return (int)cachedLength;
            }

            int incomingPackagesQueueLength = this.incomingSyncPackagesQueue.QueueLength;
            this.cache.Add("incomingPackagesQueueLength", incomingPackagesQueueLength, DateTime.Now.AddSeconds(3));

            return incomingPackagesQueueLength;
        }

        public IEnumerable<ReadSideEventHandlerDescription> GetAllAvailableHandlers()
        {
            return this.readSideAdministrationService.GetAllAvailableHandlers();
        }

        [NoTransaction]
        public ReadSideStatus GetReadSideStatus()
        {
            ReadSideStatus readSideStatus = this.readSideAdministrationService.GetRebuildStatus();
            return readSideStatus;
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