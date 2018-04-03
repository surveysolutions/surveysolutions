using System;
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
    public class when_getting_queue_length
    {
        [OneTimeSetUp]
        public void Setup()
        {
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();

            for (int i = 0; i < 100; i++)
                packagesStorage.Store(new InterviewPackage {InterviewId = Guid.NewGuid()}, null);

            interviewPackagesService = Create.Service.InterviewPackagesService(interviewPackageStorage: packagesStorage, brokenInterviewPackageStorage: brokenPackagesStorage);
            packagesLength = interviewPackagesService.QueueLength;
        }

        [Test]
        public void should_be_specified_packages_length() => packagesLength.Should().Be(100);

        private static int packagesLength;
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}