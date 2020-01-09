using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [Obsolete(@"Since v 5.10")]
    public class InterviewerApiV2Controller : ApiController
    {
        protected readonly ITabletInformationService tabletInformationService;
        protected readonly IUserViewFactory userViewFactory;
        private readonly IClientApkProvider clientApkProvider;

        public InterviewerApiV2Controller(
            ITabletInformationService tabletInformationService,
            IUserViewFactory userViewFactory,
            IClientApkProvider clientApkProvider)
        {
            this.tabletInformationService = tabletInformationService;
            this.userViewFactory = userViewFactory;
            this.clientApkProvider = clientApkProvider;
        }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            return this.clientApkProvider.GetApkAsHttpResponse(Request,
                ClientApkInfo.InterviewerFileName,
                ClientApkInfo.InterviewerResponseFileName);
        }

        [HttpGet]
        public virtual int? GetLatestVersion()
        {
            return this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.InterviewerFileName);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> PostTabletInformationAsFile()
        {
            HttpRequestMessage request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var multipartMemoryStreamProvider = await request.Content.ReadAsMultipartAsync();
            var httpContent = multipartMemoryStreamProvider.Contents.Single();
            var fileContent = await httpContent.ReadAsByteArrayAsync();

            var deviceId = this.Request.Headers.GetValues("DeviceId").Single();
            var user = this.userViewFactory.GetUser(new UserViewInputModel(deviceId));

            this.tabletInformationService.SaveTabletInformation(
                content: fileContent,
                androidId: deviceId,
                user: user);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
