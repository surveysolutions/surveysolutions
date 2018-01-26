using System;
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
    public class when_getting_invalid_packages_count
    {
        [OneTimeSetUp]
        public void Setup()
        {
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();

            for (int i = 0; i < 100; i++)
                brokenPackagesStorage.Store(new BrokenInterviewPackage { InterviewId = Guid.NewGuid() }, null);

            interviewPackagesService = Create.Service.InterviewPackagesService(interviewPackageStorage: packagesStorage, brokenInterviewPackageStorage: brokenPackagesStorage);
            
            packagesLength = interviewPackagesService.InvalidPackagesCount;
        }

        [Test]
        public void should_be_specified_packages_length() => packagesLength.ShouldEqual(100);

        private static int packagesLength;
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}