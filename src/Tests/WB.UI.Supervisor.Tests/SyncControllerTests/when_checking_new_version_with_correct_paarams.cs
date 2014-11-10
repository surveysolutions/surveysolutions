using System.Net;
using System.Net.Http;
using System.Web;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.SyncControllerTests
{
    internal class when_checking_new_version_with_correct_paarams : SyncControllerTestContext
    {
        Establish context = () =>
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.GetDirectoriesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { "dummy" });
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.GetFileName(Moq.It.IsAny<string>())).Returns(versionShouldBrFound.ToString);
            
            controller = CreateSyncControllerWithFile(fileSystemAccessor:fileSystemAccessor.Object);
        
        };

        Because of = () => 
            result = controller.CheckNewVersion(currentVersion);

        It should_return_OK_status_response = () =>
            result.StatusCode.ShouldEqual(HttpStatusCode.OK);

        private It should_return_true = () =>
            result.Content.ReadAsStringAsync().Result.ShouldBeEqualIgnoringCase("true");


        private static HttpResponseMessage result;
        private static InterviewerSyncController controller;
        private static HttpException exception;
        private static int currentVersion = 3;
        private static int versionShouldBrFound = 5;
    }
}
