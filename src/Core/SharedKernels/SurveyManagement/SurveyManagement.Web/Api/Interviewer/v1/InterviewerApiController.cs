using System;
using System.Net.Http;
using System.Web.Http;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1
{
    [ProtobufJsonSerializer]
    [Obsolete("Since v. 5.7")]
    public class InterviewerApiController : InterviewerControllerBase
    {
        public InterviewerApiController(
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
    }
}