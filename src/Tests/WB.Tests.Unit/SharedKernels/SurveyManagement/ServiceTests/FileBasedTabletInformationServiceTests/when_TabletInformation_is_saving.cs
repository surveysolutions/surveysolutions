using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.TabletInformation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.FileBasedTabletInformationServiceTests
{
    internal class when_TabletInformation_is_saving : FileBasedTabletInformationServiceTestContext
    {
        Establish context = () =>
        {
            content = new byte[] { 1, 2, 3 };

            fileBasedTabletInformationService = CreateFileBasedTabletInformationService((fileName, contentToBeSave) =>
            {
                savedFileName = fileName;
                savedContent = contentToBeSave;
            });
        };

        Because of = () => fileBasedTabletInformationService.SaveTabletInformation(content, androidId, registrationId, null);

        It should_save_content_as_it_is = () => savedContent.ShouldEqual(content);
        It should_save_zip_file = () => savedFileName.ShouldEndWith(".zip");
        It should_saved_file_contain_android_id = () => savedFileName.ShouldContain(androidId);
        It should_saved_file_contain_registration_id = () => savedFileName.ShouldContain(registrationId);
        
        private static FileBasedTabletInformationService fileBasedTabletInformationService;
        private static byte[] content;
        private static byte[] savedContent;
        private static string savedFileName;
        private static string androidId="aId";
        private static string registrationId="rId";
    }
}
