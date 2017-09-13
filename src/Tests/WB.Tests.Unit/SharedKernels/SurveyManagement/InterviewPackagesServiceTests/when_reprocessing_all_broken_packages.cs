using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    [Ignore("Fix later")]
    internal class when_reprocessing_all_broken_packages : InterviewPackagesServiceTestsContext
    {
        Establish context = () =>
        {
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();

            for (int i = 1; i <= numberOfPackages; i++)
                brokenPackagesStorage.Store(new BrokenInterviewPackage { Id = i }, null);

            interviewPackagesService = CreateInterviewPackagesService(
                interviewPackageStorage: packagesStorage, 
                brokenInterviewPackageStorage: brokenPackagesStorage);
        };

        Because of = () => interviewPackagesService.ReprocessAllBrokenPackages();

        It should_broken_packages_moved_to_packages = () =>
        {
            packagesStorage.Query(x => x.Count()).ShouldEqual(numberOfPackages);
            brokenPackagesStorage.Query(x => x.Count()).ShouldEqual(0);
        };
        
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
        private static int numberOfPackages = 100;
    }
}