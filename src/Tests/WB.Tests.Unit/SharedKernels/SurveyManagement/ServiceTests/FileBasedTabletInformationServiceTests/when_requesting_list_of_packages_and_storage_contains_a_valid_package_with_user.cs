using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.FileBasedTabletInformationServiceTests
{
    internal class when_requesting_list_of_packages_and_storage_contains_a_valid_package_with_user : FileBasedTabletInformationServiceTestContext
    {
        Establish context = () =>
        {
            fileBasedTabletInformationService = CreateFileBasedTabletInformationService(null, presentFileNames);
        };

        Because of = () => returnedPackages = fileBasedTabletInformationService.GetAllTabletInformationPackages();

        It should_contain_1_element_in_list = () => returnedPackages.Count.ShouldEqual(1);
        It should_android_id_be_equal_provided_value = () => returnedPackages[0].AndroidId.ShouldEqual(androidId);
        It should_registration_id_be_equal_5 = () => returnedPackages[0].RegistrationId.ShouldEqual("5");
        It should_username_be_as_provided = () => returnedPackages[0].UserName.ShouldEqual(userName);
        It should_userid_be_as_provided = () => returnedPackages[0].UserId.ShouldEqual(userId);

        private static FileBasedTabletInformationService fileBasedTabletInformationService;

        private static List<TabletInformationView> returnedPackages;

        private static Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static string userName = "test";
        private static string androidId = "3dqda33";
        private static string[] presentFileNames = new[]
        {
            string.Format("{0}@5@6@{1}!{2}.zip", androidId,userName,userId)
        };
    }
}
