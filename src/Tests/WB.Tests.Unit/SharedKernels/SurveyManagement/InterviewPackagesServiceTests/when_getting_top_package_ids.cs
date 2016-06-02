using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    internal class when_getting_top_package_ids : InterviewPackagesServiceTestsContext
    {
        Establish context = () =>
        {
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();

            foreach (var expectedPackageId in expectedPackageIds)
                packagesStorage.Store(new InterviewPackage { Id = expectedPackageId }, null);

            interviewPackagesService = CreateInterviewPackagesService(interviewPackageStorage: packagesStorage, brokenInterviewPackageStorage: brokenPackagesStorage);
        };

        Because of = () => actualPackageIds = interviewPackagesService.GetTopPackageIds(5);

        It should_contains_specified_package_ids = () =>
        {
            actualPackageIds.Count.ShouldEqual(expectedPackageIds.Length);
            actualPackageIds.ShouldEachConformTo(sPackageId => expectedPackageIds.Contains(int.Parse(sPackageId)));
        };

        private static IReadOnlyCollection<string> actualPackageIds;
        private static readonly int[] expectedPackageIds = { 1, 5, 7};
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}