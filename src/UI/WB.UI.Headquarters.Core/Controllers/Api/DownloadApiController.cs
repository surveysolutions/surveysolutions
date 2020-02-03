using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.API;
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
        [AllowAnonymous]
        public IActionResult GetLatestVersion()
        {
            return clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerFileName, ClientApkInfo.InterviewerResponseFileName);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetLatestExtendedVersion()
        {
            return clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerExtendedFileName, ClientApkInfo.InterviewerResponseFileName);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetLatestSupervisor()
        {
            return clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.SupervisorFileName, ClientApkInfo.SupervisorFileName);
        }
    }
}
