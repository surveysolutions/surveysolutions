using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation;
using WB.Core.BoundedContexts.Headquarters.Views.TabletInformation;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.FileBasedTabletInformationServiceTests
{
    internal class when_requesting_list_of_packages_and_storage_contains_a_valid_package_with_user : FileBasedTabletInformationServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileBasedTabletInformationService = CreateFileBasedTabletInformationService(null, presentFileNames);
            BecauseOf();
        }

        public void BecauseOf() => returnedPackages = fileBasedTabletInformationService.GetAllTabletInformationPackages();

        [NUnit.Framework.Test] public void should_contain_1_element_in_list () => returnedPackages.Count.Should().Be(1);
        [NUnit.Framework.Test] public void should_android_id_be_equal_provided_value () => returnedPackages[0].AndroidId.Should().Be(androidId);
        [NUnit.Framework.Test] public void should_username_be_as_provided () => returnedPackages[0].UserName.Should().Be(userName);
        [NUnit.Framework.Test] public void should_userid_be_as_provided () => returnedPackages[0].UserId.Should().Be(userId);

        private static FileBasedTabletInformationService fileBasedTabletInformationService;

        private static List<TabletInformationView> returnedPackages;

        private static string userId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private static string userName = "test";
        private static string androidId = "3dqda33";

        private static string[] presentFileNames = new[]
        {
            string.Format("{0}@5@6@{1}!{2}.zip", androidId, userName, userId)
        };
    }
}
