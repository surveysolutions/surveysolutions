using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation;
using WB.Core.BoundedContexts.Headquarters.Views.TabletInformation;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.FileBasedTabletInformationServiceTests
{
    internal class when_requesting_list_of_packages_and_storage_contains_2_invalid_and_1valid_package : FileBasedTabletInformationServiceTestContext
    {
        Establish context = () =>
        {
            fileBasedTabletInformationService = CreateFileBasedTabletInformationService(null, presentFileNames);
        };

        Because of = () => returnedPackages = fileBasedTabletInformationService.GetAllTabletInformationPackages();

        It should_contain_3_elements_in_list = () => returnedPackages.Count.ShouldEqual(3);
        It should_android_id_be_equal_2 = () => returnedPackages[0].AndroidId.ShouldEqual("2");

        private static FileBasedTabletInformationService fileBasedTabletInformationService;

        private static List<TabletInformationView> returnedPackages;
        private static readonly string[] presentFileNames = new[] { "1", "2.zip", "2@5@6.zip", "2@5@3@.zip", "3@3@6@a!er.zip"};
    }
}
