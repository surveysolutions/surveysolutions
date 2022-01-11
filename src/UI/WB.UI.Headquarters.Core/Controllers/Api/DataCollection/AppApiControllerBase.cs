using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Sanitizer;
using WB.UI.Shared.Web.Controllers;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public class AppControllerBaseBase : ControllerBase
    {
        // version from the sky, discussed on scrum 12/04/2019
        //revision is used to compare version of client apk
        private readonly Version LastSupportedVersion = new Version(19, 08, 0, 25531); 

        private readonly IPlainKeyValueStorage<InterviewerSettings> settingsStorage;
        private readonly IPlainStorageAccessor<ServerSettings> tenantSettings;
        private readonly IUserViewFactory userViewFactory;
        private readonly ITabletInformationService tabletInformationService;

        public AppControllerBaseBase(IPlainKeyValueStorage<InterviewerSettings> settingsStorage, 
            IPlainStorageAccessor<ServerSettings> tenantSettings, 
            IUserViewFactory userViewFactory, 
            ITabletInformationService tabletInformationService)
        {
            this.settingsStorage = settingsStorage ?? throw new ArgumentNullException(nameof(settingsStorage));
            this.tenantSettings = tenantSettings ?? throw new ArgumentNullException(nameof(tenantSettings));
            this.userViewFactory = userViewFactory;
            this.tabletInformationService = tabletInformationService;
        }

        protected bool IsNeedUpdateAppBySettings(Version appVersion, Version hqVersion)
        {
            if (appVersion == null)
                return false;

            var interviewerSettings = settingsStorage.GetById(AppSetting.InterviewerSettings);
            if (interviewerSettings.IsAutoUpdateEnabled())
            {
                return hqVersion != appVersion;
            }

            return appVersion < LastSupportedVersion;
        }

        protected bool IsNeedUpdateAppBySettings(int? clientApkBuildNumber, int? serverApkBuildNumber)
        {
            if (clientApkBuildNumber == null)
                return false;

            var interviewerSettings = settingsStorage.GetById(AppSetting.InterviewerSettings);
            if (interviewerSettings.IsAutoUpdateEnabled())
            {
                return clientApkBuildNumber != serverApkBuildNumber;
            }

            return clientApkBuildNumber < LastSupportedVersion.Revision;
        }

        protected bool UserIsFromThisTenant(string userTenantId)
        {
            if (!string.IsNullOrEmpty(userTenantId))
            {
                var serverTenantId = this.tenantSettings.GetById(ServerSettings.PublicTenantIdKey).Value;
                if (!userTenantId.Equals(serverTenantId, StringComparison.Ordinal))
                {
                    // https://httpstatuses.com/421
                    return false;
                }
            }

            return true;
        }

        [RequestSizeLimit(10L * 1024 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024* 1024 * 1024)]
        public virtual async Task<IActionResult> PostTabletInformation()
        {
            if (!Request.HasFormContentType)
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType);
            }

            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType),
                new FormOptions().MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary.ToString(), HttpContext.Request.Body);

            var section = await reader.ReadNextSectionAsync();

            if (section != null)
            {
                var formData = new MemoryStream();
                await  section.Body.CopyToAsync(formData);

                var deviceId = this.Request.Headers["DeviceId"].Single().RemoveHtmlTags();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var user = userId != null
                    ? this.userViewFactory.GetUser(new UserViewInputModel(Guid.Parse(userId)))
                    : null;

                this.tabletInformationService.SaveTabletInformation(
                    content: formData.ToArray(),
                    androidId: deviceId,
                    user: user);
            }

            return Ok();
        }
    }
}
