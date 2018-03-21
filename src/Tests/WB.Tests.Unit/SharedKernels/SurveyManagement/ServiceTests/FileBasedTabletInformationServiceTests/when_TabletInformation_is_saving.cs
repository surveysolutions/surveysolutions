using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.FileBasedTabletInformationServiceTests
{
    internal class when_TabletInformation_is_saving : FileBasedTabletInformationServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            content = new byte[] { 1, 2, 3 };

            fileBasedTabletInformationService = CreateFileBasedTabletInformationService((fileName, contentToBeSave) =>
            {
                savedFileName = fileName;
                savedContent = contentToBeSave;
            });
            BecauseOf();
        }

        public void BecauseOf() => fileBasedTabletInformationService.SaveTabletInformation(content, androidId, null);

        [NUnit.Framework.Test] public void should_save_content_as_it_is () => savedContent.Should().BeEquivalentTo(content);
        [NUnit.Framework.Test] public void should_save_zip_file () => savedFileName.Should().EndWith(".zip");
        [NUnit.Framework.Test] public void should_saved_file_contain_android_id () => savedFileName.Should().Contain(androidId);
        
        private static FileBasedTabletInformationService fileBasedTabletInformationService;
        private static byte[] content;
        private static byte[] savedContent;
        private static string savedFileName;
        private static string androidId="aId";
    }
}
