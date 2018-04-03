using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    [TestFixture]
    public class when_getting_top_package_ids
    {
        [OneTimeSetUp]
        public void Setup()
        {
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();

            foreach (var expectedPackageId in expectedPackageIds)
                packagesStorage.Store(new InterviewPackage { Id = expectedPackageId }, null);

            interviewPackagesService = Create.Service.InterviewPackagesService(interviewPackageStorage: packagesStorage, brokenInterviewPackageStorage: brokenPackagesStorage);
            actualPackageIds = interviewPackagesService.GetTopPackageIds(5);
        }

        [Test]
        public void should_contains_specified_package_ids()
        {
            actualPackageIds.Count.Should().Be(expectedPackageIds.Length);
            actualPackageIds.Should().OnlyContain(sPackageId => expectedPackageIds.Contains(int.Parse(sPackageId)));
        }

        private static IReadOnlyCollection<string> actualPackageIds;
        private static readonly int[] expectedPackageIds = { 1, 5, 7};
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}
