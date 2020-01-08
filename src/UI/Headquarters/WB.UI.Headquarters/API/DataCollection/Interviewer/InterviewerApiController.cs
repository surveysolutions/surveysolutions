using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Services;
using WB.UI.Headquarters.Utils;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer
{
    public class InterviewerApiController : AppApiControllerBase
    {
        protected readonly ITabletInformationService tabletInformationService;
        protected readonly IUserViewFactory userViewFactory;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IAssignmentsService assignmentsService;
        private readonly IClientApkProvider clientApkProvider;
        private readonly HqSignInManager signInManager;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IInterviewInformationFactory interviewFactory;
        private readonly IInterviewerVersionReader interviewerVersionReader;

        public enum ClientVersionFromUserAgent
        {
            Unknown = 0,
            WithoutMaps = 1,
            WithMaps = 2
        }

        public InterviewerApiController(ITabletInformationService tabletInformationService,
            IUserViewFactory userViewFactory,
            IInterviewerSyncProtocolVersionProvider syncVersionProvider,
            IAuthorizedUser authorizedUser,
            HqSignInManager signInManager,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IInterviewInformationFactory interviewFactory,
            IAssignmentsService assignmentsService,
            IClientApkProvider clientApkProvider,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage,
            IPlainKeyValueStorage<TenantSettings> tenantSettings,
            IInterviewerVersionReader interviewerVersionReader)
            : base(interviewerSettingsStorage, tenantSettings)
        {
            this.tabletInformationService = tabletInformationService;
            this.userViewFactory = userViewFactory;
            this.syncVersionProvider = syncVersionProvider;
            this.authorizedUser = authorizedUser;
            this.signInManager = signInManager;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.interviewFactory = interviewFactory;
            this.assignmentsService = assignmentsService;
            this.clientApkProvider = clientApkProvider;
            this.interviewerVersionReader = interviewerVersionReader;
        }
        
        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetApk)]
        public virtual HttpResponseMessage Get()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithMaps)
                return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerExtendedFileName, ClientApkInfo.InterviewerResponseFileName);

            return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerFileName, ClientApkInfo.InterviewerResponseFileName);
        }
        
        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetExtendedApk)]
        public virtual HttpResponseMessage GetExtended()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
                return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerFileName, ClientApkInfo.InterviewerResponseFileName);

            return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerExtendedFileName, ClientApkInfo.InterviewerResponseFileName);
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetApkPatch)]
        public virtual HttpResponseMessage Patch(int deviceVersion)
        {            
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if(clientVersion == ClientVersionFromUserAgent.WithMaps)
                return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.Ext.delta");

            return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.delta");
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetExtendedApkPatch)]
        public virtual HttpResponseMessage PatchExtended(int deviceVersion)
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
                return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.delta");

            return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.Ext.delta");
        }
      
        [HttpGet]
        public virtual int? GetLatestVersion()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithMaps)
                return this.clientApkProvider.GetLatestVersion(ClientApkInfo.InterviewerExtendedFileName);

            return this.clientApkProvider.GetLatestVersion(ClientApkInfo.InterviewerFileName);
        }

        [HttpGet]
        public virtual int? GetLatestExtendedVersion()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
                return this.clientApkProvider.GetLatestVersion(ClientApkInfo.InterviewerFileName);

            return this.clientApkProvider.GetLatestVersion(ClientApkInfo.InterviewerExtendedFileName);
        }

        [HttpPost]
        public virtual async Task<HttpResponseMessage> PostTabletInformation()
        {
            HttpRequestMessage request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var authHeader = request.Headers.Authorization?.ToString();

            if (authHeader != null)
            {
                await signInManager.SignInWithAuthTokenAsync(authHeader, false, UserRoles.Interviewer);
            }

            var multipartMemoryStreamProvider = await request.Content.ReadAsMultipartAsync();
            var httpContent = multipartMemoryStreamProvider.Contents.Single();
            var fileContent = await httpContent.ReadAsByteArrayAsync();

            var deviceId = this.Request.Headers.GetValues(@"DeviceId").Single();
            var userId = User.Identity.GetUserId();

            var user = userId != null
                ? this.userViewFactory.GetUser(new UserViewInputModel(Guid.Parse(userId)))
                : null;

            this.tabletInformationService.SaveTabletInformation(
                content: fileContent,
                androidId: deviceId,
                user: user);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [ApiBasicAuth(UserRoles.Interviewer)]
        [WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        [HttpGet]
        [ApiNoCache]
        public virtual HttpResponseMessage CheckCompatibility(string deviceId, int deviceSyncProtocolVersion, string tenantId = null)
        {
            int serverSyncProtocolVersion = this.syncVersionProvider.GetProtocolVersion();
            int lastNonUpdatableSyncProtocolVersion = this.syncVersionProvider.GetLastNonUpdatableVersion();

            if (deviceSyncProtocolVersion < lastNonUpdatableSyncProtocolVersion)
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);

            if (!UserIsFromThisTenant(tenantId))
            {
                return this.Request.CreateResponse(HttpStatusCode.Conflict);
            }
            
            var serverApkBuildNumber = interviewerVersionReader.Version;
            var clientApkBuildNumber = this.Request.GetBuildNumberFromUserAgent();
            
            if (clientApkBuildNumber != null && clientApkBuildNumber > serverApkBuildNumber)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }

            if (IsNeedUpdateAppBySettings(clientApkBuildNumber, serverApkBuildNumber))
            {
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
            }

            if (deviceSyncProtocolVersion < InterviewerSyncProtocolVersionProvider.ResolvedCommentsIntroduced)
            {
                if (this.interviewFactory.HasAnyInterviewsInProgressWithResolvedCommentsForInterviewer(this.authorizedUser.Id))
                {
                    return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
                }
            }

            if (deviceSyncProtocolVersion < InterviewerSyncProtocolVersionProvider.AudioRecordingIntroduced)
            {
                if (this.assignmentsService.HasAssignmentWithAudioRecordingEnabled(this.authorizedUser.Id))
                {
                    return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
                }
            }

            if (deviceSyncProtocolVersion == 7080 || deviceSyncProtocolVersion == InterviewerSyncProtocolVersionProvider.AudioRecordingIntroduced) 
                // release previous to audio recording enabled that is allowed to be synchronized
            {
            }
            else if (deviceSyncProtocolVersion == 7070) // KP-11462
            {
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
            }
            else if (deviceSyncProtocolVersion == 7060 /* pre protected questions release */)
            {
                if (deviceSyncProtocolVersion < InterviewerSyncProtocolVersionProvider.ProtectedVariablesIntroduced
                    && this.assignmentsService.HasAssignmentWithProtectedVariables(this.authorizedUser.Id))
                {
                    return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
                }
            }
            else if (deviceSyncProtocolVersion == 7050 /* PRE assignment devices, that still allowed to connect*/)
            {
                var interviewerAssignments = this.assignmentsService.GetAssignments(this.authorizedUser.Id);
                var assignedQuestionarries = this.questionnaireBrowseViewFactory.GetByIds(interviewerAssignments.Select(ia => ia.QuestionnaireId).ToArray());

                if (assignedQuestionarries.Any(aq => aq.AllowAssignments))
                {
                    return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
                }

            }
            else if (deviceSyncProtocolVersion != serverSyncProtocolVersion)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }

            return this.authorizedUser.DeviceId != deviceId
                ? this.Request.CreateResponse(HttpStatusCode.Forbidden)
                : this.Request.CreateResponse(HttpStatusCode.OK, @"449634775");
        }

        private ClientVersionFromUserAgent GetClientVersionFromUserAgent(HttpRequestMessage request)
        {
            if (request.Headers?.UserAgent != null)
            {
                foreach (var product in request.Headers?.UserAgent)
                {
                    if (product.Product?.Name.Equals(@"maps",StringComparison.OrdinalIgnoreCase)??false)
                    {
                        return ClientVersionFromUserAgent.WithMaps;
                    }
                }
                return ClientVersionFromUserAgent.WithoutMaps;
            }
            return ClientVersionFromUserAgent.Unknown;
        }
    }
}
