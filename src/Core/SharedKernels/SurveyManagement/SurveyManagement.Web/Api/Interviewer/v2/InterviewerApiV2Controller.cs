using System.Net.Http;
using System.Web.Http;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
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
    }
}