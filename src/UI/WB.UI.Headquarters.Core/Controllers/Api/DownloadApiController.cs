using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers.Api
{
    public class DownloadApiController : ControllerBase
    { 
        private readonly IClientApkProvider clientApkProvider;

        public DownloadApiController(
            IClientApkProvider clientApkProvider)
        {
            this.clientApkProvider = clientApkProvider;
        }


        [HttpGet]
        [HttpHead]
        [AllowPrimaryWorkspaceFallback]
        public Task<IActionResult> GetLatestVersion()
        {
            return clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerFileName, ClientApkInfo.InterviewerResponseFileName);
        }

        [HttpGet]
        [HttpHead]
        [AllowPrimaryWorkspaceFallback]
        public Task<IActionResult> GetLatestExtendedVersion()
        {
            return clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerExtendedFileName, ClientApkInfo.InterviewerResponseFileName);
        }

        [HttpGet]
        [HttpHead]
        [AllowPrimaryWorkspaceFallback]
        public Task<IActionResult> GetLatestSupervisor()
        {
            return clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.SupervisorFileName, ClientApkInfo.SupervisorFileName);
        }
    }
}
