using System.Collections.Generic;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation;
using WB.Core.BoundedContexts.Headquarters.Views.TabletInformation;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.FileBasedTabletInformationServiceTests
{
    internal class when_requesting_list_of_packages_and_storage_is_empty : FileBasedTabletInformationServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileBasedTabletInformationService = CreateFileBasedTabletInformationService(writeAllBytesCallback: null);
            BecauseOf();
        }

        public void BecauseOf() => returnedPackages = fileBasedTabletInformationService.GetAllTabletInformationPackages();

        [NUnit.Framework.Test] public void should_return_empty_list () => returnedPackages.Count.Should().Be(0);

        private static FileBasedTabletInformationService fileBasedTabletInformationService;

        private static List<TabletInformationView> returnedPackages;
    }
}
