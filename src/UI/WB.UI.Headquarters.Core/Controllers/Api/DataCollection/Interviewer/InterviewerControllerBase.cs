using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NHibernate.Cfg.MappingSchema;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer
{
    [Route("api/interviewer")]
    public class InterviewerControllerBase : AppControllerBaseBase
    {
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IClientApkProvider clientApkProvider;
        private readonly IInterviewerVersionReader interviewerVersionReader;
        private readonly IUserToDeviceService userToDeviceService;
        private readonly IOptions<HeadquartersConfig> hqConfig;

        public enum ClientVersionFromUserAgent
        {
            Unknown = 0,
            WithoutMaps = 1,
            WithMaps = 2
        }

        public InterviewerControllerBase(ITabletInformationService tabletInformationService,
            IUserViewFactory userViewFactory,
            IInterviewerSyncProtocolVersionProvider syncVersionProvider,
            IAuthorizedUser authorizedUser,
            IClientApkProvider clientApkProvider,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage,
            IPlainStorageAccessor<ServerSettings> tenantSettings,
            IInterviewerVersionReader interviewerVersionReader,
            IUserToDeviceService userToDeviceService,
            IOptions<HeadquartersConfig> hqConfig)
            : base(interviewerSettingsStorage, tenantSettings, userViewFactory, tabletInformationService)
        {
            this.syncVersionProvider = syncVersionProvider;
            this.authorizedUser = authorizedUser;
            this.clientApkProvider = clientApkProvider;
            this.interviewerVersionReader = interviewerVersionReader;
            this.userToDeviceService = userToDeviceService;
            this.hqConfig = hqConfig;
        }

        [HttpGet]
        [Route("")]
        [WriteToSyncLog(SynchronizationLogType.GetApk)]
        public virtual Task<IActionResult> Get()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithMaps)
                return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerExtendedFileName, ClientApkInfo.InterviewerResponseFileName);

            return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerFileName, ClientApkInfo.InterviewerResponseFileName);
        }

        [HttpGet]
        [Route("extended")]
        [WriteToSyncLog(SynchronizationLogType.GetExtendedApk)]
        public virtual Task<IActionResult> GetExtended()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
                return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerFileName, ClientApkInfo.InterviewerResponseFileName);

            return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerExtendedFileName, ClientApkInfo.InterviewerResponseFileName);
        }

        [HttpGet]
        [Route("patch/{deviceVersion:int}")]
        [WriteToSyncLog(SynchronizationLogType.GetApkPatch)]
        public virtual Task<IActionResult> Patch(int deviceVersion)
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithMaps)
                return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.Ext.delta");

            return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.delta");
        }

        [HttpGet]
        [Route("extended/patch/{deviceVersion:int}")]
        [WriteToSyncLog(SynchronizationLogType.GetExtendedApkPatch)]
        public virtual Task<IActionResult> PatchExtended(int deviceVersion)
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
                return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.delta");

            return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.Ext.delta");
        }

        [HttpGet]
        [Route("latestversion")]
        public virtual Task<int?> GetLatestVersion()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithMaps)
                return this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.InterviewerExtendedFileName);

            return this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.InterviewerFileName);
        }

        [HttpGet]
        [Route("extended/latestversion")]
        public virtual Task<int?> GetLatestExtendedVersion()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
                return this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.InterviewerFileName);

            return this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.InterviewerExtendedFileName);
        }

        [HttpPost]
        [Route("v2/tabletInfo")]
        public override Task<IActionResult> PostTabletInformation()
        {
            return base.PostTabletInformation();
        }

        [Authorize(Roles = "Interviewer")]
        [HttpGet]
        [Route("compatibility/{deviceid}/{deviceSyncProtocolVersion}")]
        [WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        public virtual async Task<IActionResult> CheckCompatibility(string deviceId, int deviceSyncProtocolVersion,
            string tenantId = null)
        {
            if (this.hqConfig.Value.IgnoreCompatibility == false)
            {
                int serverSyncProtocolVersion = this.syncVersionProvider.GetProtocolVersion();
                int lastNonUpdatableSyncProtocolVersion = this.syncVersionProvider.GetLastNonUpdatableVersion();

                if (deviceSyncProtocolVersion < lastNonUpdatableSyncProtocolVersion)
                    return StatusCode(StatusCodes.Status426UpgradeRequired);

                if (!UserIsFromThisTenant(tenantId))
                {
                    return StatusCode(StatusCodes.Status409Conflict);
                }

                var serverApkBuildNumber = await interviewerVersionReader.InterviewerBuildNumber();
                var clientApkBuildNumber = this.Request.GetBuildNumberFromUserAgent();

                if (clientApkBuildNumber != null && clientApkBuildNumber > serverApkBuildNumber)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable);
                }

                if (clientApkBuildNumber != null && this.syncVersionProvider.GetBlackListedBuildNumbers().Contains(clientApkBuildNumber.Value))
                {
                    return StatusCode(StatusCodes.Status426UpgradeRequired);
                }

                if (IsNeedUpdateAppBySettings(clientApkBuildNumber, serverApkBuildNumber))
                {
                    return StatusCode(StatusCodes.Status426UpgradeRequired);
                }

                if (deviceSyncProtocolVersion < InterviewerSyncProtocolVersionProvider.ResolvedCommentsIntroduced)
                {
                    return StatusCode(StatusCodes.Status426UpgradeRequired);
                }

                if (deviceSyncProtocolVersion < InterviewerSyncProtocolVersionProvider.WorkspacesIntroduced)
                {
                    if (this.authorizedUser.HasNonDefaultWorkspace)
                    {
                        return StatusCode(StatusCodes.Status426UpgradeRequired);
                    }   
                }
                if (deviceSyncProtocolVersion < InterviewerSyncProtocolVersionProvider.ResetPasswordIntroduced)
                {
                    if (authorizedUser.PasswordChangeRequired)
                        return StatusCode(StatusCodes.Status426UpgradeRequired);
                }          
                if (deviceSyncProtocolVersion < InterviewerSyncProtocolVersionProvider.MultiWorkspacesIntroduced)
                {
                    if (authorizedUser.Workspaces.Count() > 1)
                        return StatusCode(StatusCodes.Status426UpgradeRequired);
                }
                if (deviceSyncProtocolVersion > serverSyncProtocolVersion)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable);
                }
            }
            return this.userToDeviceService.GetLinkedDeviceId(this.authorizedUser.Id) != deviceId
                ? (IActionResult)StatusCode(StatusCodes.Status403Forbidden, new { Message = "relinked" })
                : new JsonResult("449634775");
        }

        private ClientVersionFromUserAgent GetClientVersionFromUserAgent(HttpRequest request)
        {
            if (request.Headers.ContainsKey(HeaderNames.UserAgent))
            {
                foreach (var product in request.Headers[HeaderNames.UserAgent])
                {
                    if (product.Contains("maps", StringComparison.OrdinalIgnoreCase))
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
