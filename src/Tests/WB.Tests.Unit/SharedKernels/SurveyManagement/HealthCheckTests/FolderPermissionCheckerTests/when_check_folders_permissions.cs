using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests.FolderPermissionCheckerTests
{
    [Subject(typeof(FolderPermissionChecker))]
    internal class when_check_folders_permissions
    {
        Establish context = () =>
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.GetDirectoriesInDirectory(Moq.It.IsAny<string>()))
                .Returns(new[] {"1", "2", "3"});
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>()))
               .Returns(true);
            fileSystemAccessor.Setup(x => x.GetFileName(Moq.It.IsAny<string>()))
              .Returns<string>(s=>s);
            fileSystemAccessor.Setup(x => x.IsWritePermissionExists(Moq.It.Is<string>(path => deniedFolders.Contains(path))))
                .Returns(false);
            fileSystemAccessor.Setup(x => x.IsWritePermissionExists(Moq.It.Is<string>(path => allowedFolders.Contains(path))))
              .Returns(true);

            folderPermissionChecker = new FolderPermissionChecker(rootFolder, fileSystemAccessor.Object);
        };

        Because of = () =>
            result = folderPermissionChecker.Check();

        It should_return_3_allowed_folders = () =>
            result.AllowedFolders.ShouldEqual(allowedFolders);

        It should_return_1_denied_folder = () =>
          result.DeniedFolders.ShouldEqual(deniedFolders);

        It should_return_Down_status = () =>
         result.Status.ShouldEqual(HealthCheckStatus.Down);

        private static FolderPermissionChecker folderPermissionChecker;
        private static FolderPermissionCheckResult result;

        private static string rootFolder = "root";
        private static string[] allowedFolders = new[] { rootFolder, "1", "2" };
        private static string[] deniedFolders = new[] { "3" };
    }
}
