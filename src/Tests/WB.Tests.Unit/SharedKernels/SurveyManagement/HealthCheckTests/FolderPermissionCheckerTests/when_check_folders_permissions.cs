using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;
using WB.Core.Infrastructure.FileSystem;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests.FolderPermissionCheckerTests
{
    [NUnit.Framework.TestOf(typeof(FolderPermissionChecker))]
    internal class when_check_folders_permissions
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
            BecauseOf();
        }

        public void BecauseOf() =>
            result = folderPermissionChecker.Check();

        [NUnit.Framework.Test] public void should_return_3_allowed_folders () =>
            result.AllowedFolders.Should().BeEquivalentTo(allowedFolders);

        [NUnit.Framework.Test] public void should_return_1_denied_folder () =>
          result.DeniedFolders.Should().BeEquivalentTo(deniedFolders);

        [NUnit.Framework.Test] public void should_return_Down_status () =>
         result.Status.Should().Be(HealthCheckStatus.Down);

        private static FolderPermissionChecker folderPermissionChecker;
        private static FolderPermissionCheckResult result;

        private static string rootFolder = "root";
        private static string[] allowedFolders = new[] { rootFolder, "1", "2" };
        private static string[] deniedFolders = new[] { "3" };
    }
}
