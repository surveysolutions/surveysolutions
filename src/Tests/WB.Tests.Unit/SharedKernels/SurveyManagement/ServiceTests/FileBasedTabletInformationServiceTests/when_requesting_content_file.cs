using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.FileBasedTabletInformationServiceTests
{
    internal class when_requesting_content_file : FileBasedTabletInformationServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileBasedTabletInformationService = CreateFileBasedTabletInformationService(writeAllBytesCallback: null);
            BecauseOf();
        }

        public void BecauseOf() => returnedPackagePath = fileBasedTabletInformationService.GetFullPathToContentFile(packageName);

        [NUnit.Framework.Test] public void should_result_ends_with_packageName () => returnedPackagePath.Should().EndWith(packageName);

        private static FileBasedTabletInformationService fileBasedTabletInformationService;
        
        private static string packageName = "packName";
        private static string returnedPackagePath;
    }
}
