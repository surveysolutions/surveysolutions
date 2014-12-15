using System.Net;
using System.Net.Http;
using System.Web;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Supervisor.SyncControllerTests
{
    internal class when_checking_new_version_with_correct_paarams : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.GetDirectoriesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { "dummy" });
            fileSystemAccessor.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);

            var versionProvider = Mock.Of<ISupportedVersionProvider>(x => x.GetApplicationBuildNumber() == versionShouldBeFound);

            controller = CreateSyncControllerWithFile(fileSystemAccessor: fileSystemAccessor.Object, versionProvider: versionProvider);
        
        };

        Because of = () => 
            result = controller.CheckNewVersion(currentVersion);
        
        It should_return_true = () =>
            result.ShouldBeTrue();


        private static bool result;
        private static InterviewerSyncController controller;
        private static int currentVersion = 3;
        private static int versionShouldBeFound = 5;
    }
}
