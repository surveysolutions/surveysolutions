﻿using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Route("api/supervisor")]
    public class SupervisorControllerBase : AppControllerBaseBase
    {
        private readonly ITabletInformationService tabletInformationService;
        private readonly ISupervisorSyncProtocolVersionProvider syncVersionProvider;
        private readonly IUserViewFactory userViewFactory;
        private readonly IClientApkProvider clientApkProvider;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IInterviewInformationFactory interviewFactory;
        private readonly IInterviewerVersionReader interviewerVersionReader;

        public SupervisorControllerBase(ITabletInformationService tabletInformationService, 
            ISupervisorSyncProtocolVersionProvider syncVersionProvider,
            IUserViewFactory userViewFactory, 
            IPlainKeyValueStorage<InterviewerSettings> settingsStorage,
            IPlainKeyValueStorage<TenantSettings> tenantSettings,
            IClientApkProvider clientApkProvider,
            IAuthorizedUser authorizedUser,
            IInterviewInformationFactory interviewFactory,
            IInterviewerVersionReader interviewerVersionReader)
            : base(settingsStorage, tenantSettings)
        {
            this.tabletInformationService = tabletInformationService;
            this.syncVersionProvider = syncVersionProvider;
            this.userViewFactory = userViewFactory;
            this.clientApkProvider = clientApkProvider;
            this.authorizedUser = authorizedUser;
            this.interviewFactory = interviewFactory;
            this.interviewerVersionReader = interviewerVersionReader;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetSupervisorApk)]
        [Route("v1/extended")]
        public virtual IActionResult GetSupervisor() =>
            this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.SupervisorFileName, ClientApkInfo.SupervisorFileName);

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetApk)]
        [Route("v1/apk/interviewer")]
        public virtual IActionResult GetInterviewer() =>
            this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerFileName, ClientApkInfo.InterviewerFileName);

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetExtendedApk)]
        [Route("v1/apk/interviewer-with-maps")]
        public virtual IActionResult GetInterviewerWithMaps() =>
            this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerExtendedFileName, ClientApkInfo.InterviewerFileName);
        
        [HttpGet]
        [Route("v1/extended/latestversion")]
        public virtual int? GetLatestVersion()
        {
            return this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.SupervisorFileName);
        }

        [Authorize(Roles = "Supervisor")]
        [WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        [HttpGet]
        [Route("compatibility/{deviceid}/{deviceSyncProtocolVersion}")]
        public virtual IActionResult CheckCompatibility(string deviceId, int deviceSyncProtocolVersion, string tenantId = null)
        {
            int serverSyncProtocolVersion = this.syncVersionProvider.GetProtocolVersion();
            int lastNonUpdatableSyncProtocolVersion = this.syncVersionProvider.GetLastNonUpdatableVersion();
            if (deviceSyncProtocolVersion < lastNonUpdatableSyncProtocolVersion)
                return StatusCode(StatusCodes.Status426UpgradeRequired);

            if (!UserIsFromThisTenant(tenantId))
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }
            
            if (deviceSyncProtocolVersion < SupervisorSyncProtocolVersionProvider.V2_ResolvedCommentsIntroduced)
            {
                if (this.interviewFactory.HasAnyInterviewsInProgressWithResolvedCommentsForSupervisor(
                    this.authorizedUser.Id))
                {
                    return StatusCode(StatusCodes.Status426UpgradeRequired);
                }
            }

            var serverApkBuildNumber = interviewerVersionReader.SupervisorBuildNumber;
            var clientApkBuildNumber = this.Request.GetBuildNumberFromUserAgent();
            
            if (IsNeedUpdateAppBySettings(clientApkBuildNumber, serverApkBuildNumber))
            {
                return StatusCode(StatusCodes.Status426UpgradeRequired);
            }

            if (clientApkBuildNumber != null && clientApkBuildNumber > serverApkBuildNumber)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }

            if (deviceSyncProtocolVersion == SupervisorSyncProtocolVersionProvider.V1_BeforeResolvedCommentsIntroduced) 
            {
                // allowed to synchronize
            }
            else if (deviceSyncProtocolVersion != serverSyncProtocolVersion)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }
            
            return new JsonResult("158329303");
        }

        [HttpPost]
        [Route("v1/tabletInfo")]
        public async Task<IActionResult> PostTabletInformation(IFormFile formFile)
        {
            if (formFile == null)
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType);
            }

            var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);

            var deviceId = this.Request.Headers[@"DeviceId"].Single();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = userId != null
                ? this.userViewFactory.GetUser(new UserViewInputModel(Guid.Parse(userId)))
                : null;

            this.tabletInformationService.SaveTabletInformation(
                content: memoryStream.ToArray(),
                androidId: deviceId,
                user: user);

            return Ok();
        }
     
        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetSupervisorApkPatch)]
        [Route("v1/extended/patch/{deviceVersion}")]
        public virtual IActionResult Patch(int deviceVersion)
        {
            return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"Supervisor.{deviceVersion}.delta");
        }
    }
}
