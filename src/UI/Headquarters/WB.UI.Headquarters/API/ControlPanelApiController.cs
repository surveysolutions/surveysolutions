﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;


namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [LocalOrDevelopmentAccessOnly]
    [NoTransaction]
    public class ControlPanelApiController : ApiController
    {
        public class VersionsInfo
        {
            public VersionsInfo(string product, int readSideApplication, int? readSideDatabase, Dictionary<DateTime, string> history)
            {
                this.Product = product;
                this.ReadSide_Application = readSideApplication;
                this.ReadSide_Database = readSideDatabase;
                this.History = history;
            }

            public string Product { get; }
            public int ReadSide_Application { get; }
            public int? ReadSide_Database { get; }
            public Dictionary<DateTime, string> History { get; }
        }

        private const string DEFAULTEMPTYQUERY = "";
        private const int DEFAULTPAGESIZE = 12;

        private readonly IReadSideAdministrationService readSideAdministrationService;
        private readonly IInterviewPackagesService incomingSyncPackagesQueue;
        private readonly ISynchronizationLogViewFactory synchronizationLogViewFactory;
        private readonly ITeamViewFactory teamViewFactory;
        private readonly IBrokenInterviewPackagesViewFactory brokenInterviewPackagesViewFactory;
        private readonly MemoryCache cache = MemoryCache.Default;
        private readonly IProductVersion productVersion;
        private readonly IProductVersionHistory productVersionHistory;
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly IInterviewPackagesService interviewPackagesService;


        public ControlPanelApiController(
            IReadSideAdministrationService readSideAdministrationService,
            IInterviewPackagesService incomingSyncPackagesQueue,
            ISynchronizationLogViewFactory synchronizationLogViewFactory,
            ITeamViewFactory teamViewFactory,
            IBrokenInterviewPackagesViewFactory brokenInterviewPackagesViewFactory,
            IProductVersion productVersion,
            IProductVersionHistory productVersionHistory,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory,
            IInterviewPackagesService interviewPackagesService)
        {
            this.readSideAdministrationService = readSideAdministrationService;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.synchronizationLogViewFactory = synchronizationLogViewFactory;
            this.teamViewFactory = teamViewFactory;
            this.brokenInterviewPackagesViewFactory = brokenInterviewPackagesViewFactory;
            this.productVersion = productVersion;
            this.productVersionHistory = productVersionHistory;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.interviewPackagesService = interviewPackagesService;
        }
        
        [NoTransaction]
        public VersionsInfo GetVersions()
        {
            var readSideStatus = this.readSideAdministrationService.GetRebuildStatus();

            return new VersionsInfo(
                this.productVersion.ToString(),
                readSideStatus.ReadSideApplicationVersion,
                readSideStatus.ReadSideDatabaseVersion,
                this.productVersionHistory.GetHistory().ToDictionary(
                    change => change.UpdateTimeUtc,
                    change => change.ProductVersion));
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

        [HttpGet]
        public int GetIncomingPackagesQueueLength() => this.incomingSyncPackagesQueue.QueueLength;

        [HttpPost]
        public SynchronizationLog GetSynchronizationLog(SynchronizationLogFilter filter) => this.synchronizationLogViewFactory.GetLog(filter);

        [HttpGet]
        public UsersView SyncLogInterviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.synchronizationLogViewFactory.GetInterviewers(pageSize: pageSize, searchBy: query);

        [HttpGet]
        public SynchronizationLogDevicesView SyncLogDevices(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.synchronizationLogViewFactory.GetDevices(pageSize: pageSize, searchBy: query);

        [HttpPost]
        public BrokenInterviewPackagesView GetBrokenInterviewPackages(BrokenInterviewPackageFilter filter)
            => this.brokenInterviewPackagesViewFactory.GetFilteredItems(filter);

        [HttpGet]
        public BrokenInterviewPackageExceptionTypesView GetBrokenInterviewPackageExceptionTypes(
            string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.brokenInterviewPackagesViewFactory.GetExceptionTypes(pageSize: pageSize, searchBy: query);

        [HttpGet]
        public UsersView Interviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.teamViewFactory.GetAllInterviewers(pageSize: pageSize, searchBy: query);

        [HttpGet]
        public IEnumerable<QuestionnaireView> Questionnaires()
            => this.allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel()).Questionnaires?.Select(questionnaire=>new QuestionnaireView
            {
                Identity = new QuestionnaireIdentity(questionnaire.TemplateId, questionnaire.TemplateVersion).ToString(),
                Title = $"(ver. {questionnaire.TemplateVersion}) {questionnaire.TemplateName}"
            });

        [HttpPost]
        public void ReprocessBrokenPackages() => this.interviewPackagesService.ReprocessAllBrokenPackages();

        [HttpPost]
        public void ReprocessSelectedBrokenPackages(ReprocessSelectedBrokenPackagesRequestView request) 
            => this.interviewPackagesService.ReprocessSelectedBrokenPackages(request.PackageIds);

        public class ReprocessSelectedBrokenPackagesRequestView
        {
            public int[] PackageIds { get; set; }
        }

        public class QuestionnaireView
        {
            public string Title{get; set; }
            public string Identity { get; set; }
        }
    }
}