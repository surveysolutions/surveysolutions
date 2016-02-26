using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Http;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
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
        private const string DEFAULTEMPTYQUERY = "";
        private const int DEFAULTPAGESIZE = 12;

        private readonly IReadSideAdministrationService readSideAdministrationService;
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private readonly ISynchronizationLogViewFactory synchronizationLogViewFactory;
        private readonly ITeamViewFactory teamViewFactory;
        private readonly MemoryCache cache = MemoryCache.Default;
        private readonly IProductVersion productVersion;
        private readonly IProductVersionHistory productVersionHistory;


        public ControlPanelApiController(
            IReadSideAdministrationService readSideAdministrationService,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue,
            ISynchronizationLogViewFactory synchronizationLogViewFactory,
            ITeamViewFactory teamViewFactory,
            IProductVersion productVersion,
            IProductVersionHistory productVersionHistory)
        {
            this.readSideAdministrationService = readSideAdministrationService;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.synchronizationLogViewFactory = synchronizationLogViewFactory;
            this.teamViewFactory = teamViewFactory;
            this.productVersion = productVersion;
            this.productVersionHistory = productVersionHistory;
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
            => this.readSideAdministrationService.GetAllAvailableHandlers();

        [NoTransaction]
        public ReadSideStatus GetReadSideStatus() => this.readSideAdministrationService.GetRebuildStatus();

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
        public void StopReadSideRebuilding() => this.readSideAdministrationService.StopAllViewsRebuilding();

        [HttpPost]
        public SynchronizationLog GetSynchronizationLog(SynchronizationLogFilter filter)
        {
            return this.synchronizationLogViewFactory.GetLog(filter);
        }

        [HttpGet]
        public UsersView SyncLogInterviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
        {
            return this.synchronizationLogViewFactory.GetInterviewers(pageSize: pageSize, searchBy: query);
        }

        [HttpGet]
        public SynchronizationLogDevicesView SyncLogDevices(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
        {
            return this.synchronizationLogViewFactory.GetDevices(pageSize: pageSize, searchBy: query);
        }
    }
}