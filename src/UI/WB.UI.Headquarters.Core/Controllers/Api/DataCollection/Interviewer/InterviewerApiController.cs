using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer
{
    public class InterviewerApiController : AppApiControllerBase
    {
        protected readonly ITabletInformationService tabletInformationService;
        protected readonly IUserViewFactory userViewFactory;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IProductVersion productVersion;
        private readonly IAssignmentsService assignmentsService;
        private readonly IClientApkProvider clientApkProvider;
        private readonly SignInManager<HqUser> signInManager;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IInterviewInformationFactory interviewFactory;
        private IUserToDeviceService userToDeviceService;
        
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
            IProductVersion productVersion,
            SignInManager<HqUser> signInManager,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IInterviewInformationFactory interviewFactory,
            IAssignmentsService assignmentsService,
            IClientApkProvider clientApkProvider,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage,
            IPlainKeyValueStorage<TenantSettings> tenantSettings,
            IUserToDeviceService userToDeviceService)
            : base(interviewerSettingsStorage, tenantSettings)
        {
            this.tabletInformationService = tabletInformationService;
            this.userViewFactory = userViewFactory;
            this.syncVersionProvider = syncVersionProvider;
            this.authorizedUser = authorizedUser;
            this.productVersion = productVersion;
            this.signInManager = signInManager;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.interviewFactory = interviewFactory;
            this.assignmentsService = assignmentsService;
            this.clientApkProvider = clientApkProvider;
            this.userToDeviceService = userToDeviceService;
        }
        
        //[HttpGet]
        //public virtual IActionResult Get()
        //{
        //    var clientVersion = GetClientVersionFromUserAgent(this.Request);
        //    if (clientVersion == ClientVersionFromUserAgent.WithMaps)
        //        return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerExtendedFileName, ClientApkInfo.InterviewerResponseFileName);

        //    return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerFileName, ClientApkInfo.InterviewerResponseFileName);
        //}
        
        //[HttpGet]
        //public virtual IActionResult GetExtended()
        //{
        //    var clientVersion = GetClientVersionFromUserAgent(this.Request);
        //    if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
        //        return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerFileName, ClientApkInfo.InterviewerResponseFileName);

        //    return this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerExtendedFileName, ClientApkInfo.InterviewerResponseFileName);
        //}

        //[HttpGet]
        //public virtual IActionResult Patch(int deviceVersion)
        //{            
        //    var clientVersion = GetClientVersionFromUserAgent(this.Request);
        //    if(clientVersion == ClientVersionFromUserAgent.WithMaps)
        //        return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.Ext.delta");

        //    return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.delta");
        //}

        //[HttpGet]
        //public virtual IActionResult PatchExtended(int deviceVersion)
        //{
        //    var clientVersion = GetClientVersionFromUserAgent(this.Request);
        //    if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
        //        return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.delta");

        //    return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"WBCapi.{deviceVersion}.Ext.delta");
        //}
      
        //[HttpGet]
        //public virtual int? GetLatestVersion()
        //{
        //    var clientVersion = GetClientVersionFromUserAgent(this.Request);
        //    if (clientVersion == ClientVersionFromUserAgent.WithMaps)
        //        return this.clientApkProvider.GetLatestVersion(ClientApkInfo.InterviewerExtendedFileName);

        //    return this.clientApkProvider.GetLatestVersion(ClientApkInfo.InterviewerFileName);
        //}

        //[HttpGet]
        //public virtual int? GetLatestExtendedVersion()
        //{
        //    var clientVersion = GetClientVersionFromUserAgent(this.Request);
        //    if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
        //        return this.clientApkProvider.GetLatestVersion(ClientApkInfo.InterviewerFileName);

        //    return this.clientApkProvider.GetLatestVersion(ClientApkInfo.InterviewerExtendedFileName);
        //}

        //[HttpPost]
        //public virtual async Task<IActionResult> PostTabletInformation(IFormFile formFile)
        //{
        //    if (formFile == null)
        //    {
        //        return StatusCode(StatusCodes.Status415UnsupportedMediaType);
        //    }

        //    if (Request.Headers.ContainsKey(HeaderNames.Authorization))
        //    {
        //        var authHeader = Request.Headers[HeaderNames.Authorization].ToString();

        //        await signInManager.SignInWithAuthTokenAsync(authHeader, false, UserRoles.Interviewer);
        //    }

        //    var formData = new MemoryStream();
        //    await formFile.CopyToAsync(formData);

        //    var deviceId = this.Request.Headers["DeviceId"].Single();

        //    var userId = User.Identity.GetUserId();

        //    var user = userId != null
        //        ? this.userViewFactory.GetUser(new UserViewInputModel(Guid.Parse(userId)))
        //        : null;

        //    this.tabletInformationService.SaveTabletInformation(
        //        content: formData.ToArray(),
        //        androidId: deviceId,
        //        user: user);

        //    return Ok();
        //}

        [Authorize(Roles = "Interviewer")]
        [HttpGet]
        [Route("api/interviewer/compatibility/{deviceid}/{deviceSyncProtocolVersion}")]
        public virtual IActionResult CheckCompatibility(string deviceId, int deviceSyncProtocolVersion, string tenantId = null)
        {
            int serverSyncProtocolVersion = this.syncVersionProvider.GetProtocolVersion();
            int lastNonUpdatableSyncProtocolVersion = this.syncVersionProvider.GetLastNonUpdatableVersion();

            if (deviceSyncProtocolVersion < lastNonUpdatableSyncProtocolVersion)
                return StatusCode((int) HttpStatusCode.UpgradeRequired);

            if (!UserIsFromThisTenant(tenantId))
            {
                return StatusCode((int) HttpStatusCode.Conflict);
            }

            var currentVersion = new Version(this.productVersion.ToString().Split(' ')[0]);
            var interviewerVersion = this.Request.GetProductVersionFromUserAgent(@"org.worldbank.solutions.interviewer");

            if (interviewerVersion != null && interviewerVersion > currentVersion)
            {
                return StatusCode((int) HttpStatusCode.NotAcceptable);
            }

            if (IsNeedUpdateAppBySettings(interviewerVersion, currentVersion))
            {
                return StatusCode((int) HttpStatusCode.UpgradeRequired);
            }

            if (deviceSyncProtocolVersion < InterviewerSyncProtocolVersionProvider.ResolvedCommentsIntroduced)
            {
                if (this.interviewFactory.HasAnyInterviewsInProgressWithResolvedCommentsForInterviewer(this.authorizedUser.Id))
                {
                    return StatusCode((int) HttpStatusCode.UpgradeRequired);
                }
            }

            if (deviceSyncProtocolVersion < InterviewerSyncProtocolVersionProvider.AudioRecordingIntroduced)
            {
                if (this.assignmentsService.HasAssignmentWithAudioRecordingEnabled(this.authorizedUser.Id))
                {
                    return StatusCode((int) HttpStatusCode.UpgradeRequired);
                }
            }

            if (deviceSyncProtocolVersion == 7080 || deviceSyncProtocolVersion == InterviewerSyncProtocolVersionProvider.AudioRecordingIntroduced) 
                // release previous to audio recording enabled that is allowed to be synchronized
            {
            }
            else if (deviceSyncProtocolVersion == 7070) // KP-11462
            {
                return StatusCode((int) HttpStatusCode.UpgradeRequired);
            }
            else if (deviceSyncProtocolVersion == 7060 /* pre protected questions release */)
            {
                if (deviceSyncProtocolVersion < InterviewerSyncProtocolVersionProvider.ProtectedVariablesIntroduced
                    && this.assignmentsService.HasAssignmentWithProtectedVariables(this.authorizedUser.Id))
                {
                    return StatusCode((int) HttpStatusCode.UpgradeRequired);
                }
            }
            else if (deviceSyncProtocolVersion == 7050 /* PRE assignment devices, that still allowed to connect*/)
            {
                var interviewerAssignments = this.assignmentsService.GetAssignments(this.authorizedUser.Id);
                var assignedQuestionarries = this.questionnaireBrowseViewFactory.GetByIds(interviewerAssignments.Select(ia => ia.QuestionnaireId).ToArray());

                if (assignedQuestionarries.Any(aq => aq.AllowAssignments))
                {
                    return StatusCode((int) HttpStatusCode.UpgradeRequired);
                }

            }
            else if (deviceSyncProtocolVersion != serverSyncProtocolVersion)
            {
                return StatusCode((int) HttpStatusCode.NotAcceptable);
            }

            return this.userToDeviceService.GetLinkedDeviceId(this.authorizedUser.Id) != deviceId
                ? (IActionResult) Forbid()
                : new JsonResult("449634775");
        }

        //private ClientVersionFromUserAgent GetClientVersionFromUserAgent(HttpRequest request)
        //{
        //    if (request.Headers?.UserAgent != null)
        //    {
        //        foreach (var product in request.Headers?.UserAgent)
        //        {
        //            if (product.Product?.Name.Equals(@"maps",StringComparison.OrdinalIgnoreCase)??false)
        //            {
        //                return ClientVersionFromUserAgent.WithMaps;
        //            }
        //        }
        //        return ClientVersionFromUserAgent.WithoutMaps;
        //    }
        //    return ClientVersionFromUserAgent.Unknown;
        //}
    }
}
