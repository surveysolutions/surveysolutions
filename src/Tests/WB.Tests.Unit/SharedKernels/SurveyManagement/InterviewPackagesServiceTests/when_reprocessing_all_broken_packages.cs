using System.Linq;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    [TestFixture]
    internal class when_reprocessing_all_broken_packages
    {
        [OneTimeSetUp]
        public void Setup()
        {
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();

            for (int i = 1; i <= numberOfPackages; i++)
                brokenPackagesStorage.Store(new BrokenInterviewPackage { Id = i }, null);

            interviewPackagesService = Create.Service.InterviewPackagesService(
                syncSettings: Create.Entity.SyncSettings(useBackgroundJobForProcessingPackages: true),
                interviewPackageStorage: packagesStorage, 
                brokenInterviewPackageStorage: brokenPackagesStorage);

            interviewPackagesService.ReprocessAllBrokenPackages();
        }

        [Test]
        public void should_broken_packages_moved_to_packages()
        {
            packagesStorage.Query(x => x.Count()).ShouldEqual(numberOfPackages);
            brokenPackagesStorage.Query(x => x.Count()).ShouldEqual(0);
        }
        
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
        private static int numberOfPackages = 100;
    }
}