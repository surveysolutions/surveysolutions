using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Events;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Shared.Web.Filters;


namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [ControlPanelAccess]
    public class ControlPanelApiController : ApiController
    {
        public class VersionsInfo
        {
            public VersionsInfo(string product, Dictionary<DateTime, string> history)
            {
                this.Product = product;
                this.History = history;
            }

            public string Product { get; }
            public Dictionary<DateTime, string> History { get; }
        }

        private const string DEFAULTEMPTYQUERY = "";
        private const int DEFAULTPAGESIZE = 12;

        private readonly IInterviewPackagesService incomingSyncPackagesQueue;
        private readonly ISynchronizationLogViewFactory synchronizationLogViewFactory;
        private readonly IBrokenInterviewPackagesViewFactory brokenInterviewPackagesViewFactory;
        private readonly IProductVersion productVersion;
        private readonly IProductVersionHistory productVersionHistory;
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly IInterviewBrokenPackagesService interviewBrokenPackagesService;
        private readonly IUserViewFactory userViewFactory;
        private readonly IJsonAllTypesSerializer serializer;

        public ControlPanelApiController(
            IInterviewPackagesService incomingSyncPackagesQueue,
            ISynchronizationLogViewFactory synchronizationLogViewFactory,
            IBrokenInterviewPackagesViewFactory brokenInterviewPackagesViewFactory,
            IProductVersion productVersion,
            IProductVersionHistory productVersionHistory,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory,
            IInterviewBrokenPackagesService interviewBrokenPackagesService,
            IUserViewFactory userViewFactory, 
            IJsonAllTypesSerializer serializer)
        {
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.synchronizationLogViewFactory = synchronizationLogViewFactory;
            this.brokenInterviewPackagesViewFactory = brokenInterviewPackagesViewFactory;
            this.productVersion = productVersion;
            this.productVersionHistory = productVersionHistory;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.interviewBrokenPackagesService = interviewBrokenPackagesService;
            this.userViewFactory = userViewFactory;
            this.serializer = serializer;
        }
        
        public VersionsInfo GetVersions()
        {
            return new VersionsInfo(
                this.productVersion.ToString(),
                this.productVersionHistory.GetHistory().ToDictionary(
                    change => change.UpdateTimeUtc,
                    change => change.ProductVersion));
        }

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

        [HttpGet]
        [ApiNoCache]
        public BrokenInterviewPackageExceptionTypesView GetRejectedInterviewPackageExceptionTypes(
            string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.brokenInterviewPackagesViewFactory.GetExceptionTypes(pageSize: pageSize, searchBy: query);

        [HttpPost]
        public BrokenInterviewPackagesView GetBrokenInterviewPackages(BrokenInterviewPackageFilter filter)
            => this.brokenInterviewPackagesViewFactory.GetFilteredItems(filter);

        [HttpPost]
        public BrokenInterviewPackagesView GetRejectedInterviewPackages(BrokenInterviewPackageFilter filter)
        {
            filter.ReturnOnlyUnknownExceptionType = true;
            return this.brokenInterviewPackagesViewFactory.GetFilteredItems(filter);
        }

        [HttpGet]
        public UsersView Interviewers(string query = DEFAULTEMPTYQUERY, int pageSize = DEFAULTPAGESIZE)
            => this.userViewFactory.GetInterviewers(pageSize: pageSize, searchBy: query, supervisorId: null);

        [HttpGet]
        public IEnumerable<QuestionnaireView> Questionnaires()
            => this.allUsersAndQuestionnairesFactory.Load().Questionnaires?.Select(questionnaire=>new QuestionnaireView
            {
                Identity = new QuestionnaireIdentity(questionnaire.TemplateId, questionnaire.TemplateVersion).ToString(),
                Title = $"(ver. {questionnaire.TemplateVersion}) {questionnaire.TemplateName}"
            });

        [HttpPost]
        public void ReprocessSelectedBrokenPackages(ReprocessSelectedBrokenPackagesRequestView request) 
            => this.interviewBrokenPackagesService.ReprocessSelectedBrokenPackages(request.PackageIds);

        [HttpGet]
        [ApiNoCache]
        public IHttpActionResult DownloadSyncPackage(int id, string format)
        {
            BrokenInterviewPackage interviewPackage = this.brokenInterviewPackagesViewFactory.GetPackage(id);
            if ("json".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                var events = this.serializer.Deserialize<AggregateRootEvent[]>(interviewPackage.Events);
                return Json(events, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Objects
                });
            }
            return Content(HttpStatusCode.OK, interviewPackage.Events);
        }

        [System.Web.Http.HttpPost]
        public HttpResponseMessage MarkReasonAsKnown(MarkKnownReasonRequest request)
        {
            this.interviewBrokenPackagesService.PutReason(request.PackageIds, request.ErrorType);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public class ReprocessSelectedBrokenPackagesRequestView
        {
            public int[] PackageIds { get; set; }
        }

        public class MarkKnownReasonRequest
        {
            public int[] PackageIds { get; set; }

            public InterviewDomainExceptionType ErrorType { get; set; }
        }

        public class QuestionnaireView
        {
            public string Title{get; set; }
            public string Identity { get; set; }
        }
    }
}
