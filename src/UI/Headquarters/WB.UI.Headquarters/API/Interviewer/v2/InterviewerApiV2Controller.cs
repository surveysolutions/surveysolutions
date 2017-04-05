using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.TabletInformation;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [Obsolete(@"Since v 5.10")]
    public class InterviewerApiV2Controller : InterviewerControllerBase
    {
        public InterviewerApiV2Controller(
            IFileSystemAccessor fileSystemAccessor,
            ITabletInformationService tabletInformationService,
            IUserViewFactory userViewFactory,
            IAndroidPackageReader androidPackageReader) : base(
                fileSystemAccessor: fileSystemAccessor,
                tabletInformationService: tabletInformationService,
                androidPackageReader: androidPackageReader,
                userViewFactory: userViewFactory)
        {
        }

        [HttpGet]
        public override HttpResponseMessage Get() => base.Get();

        [HttpGet]
        public override int? GetLatestVersion() => base.GetLatestVersion();

        [HttpPost]
        public override HttpResponseMessage PostTabletInformation(TabletInformationPackage tabletInformationPackage) => base.PostTabletInformation(tabletInformationPackage);

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