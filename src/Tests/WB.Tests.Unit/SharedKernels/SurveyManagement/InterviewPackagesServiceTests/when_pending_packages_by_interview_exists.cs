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
    public class when_pending_packages_by_interview_exists
    {
        [OneTimeSetUp]
        public void Setup()
        {
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();

            packagesStorage.Store(new InterviewPackage {InterviewId = interviewId}, null);
            
            interviewPackagesService = Create.Service.InterviewPackagesService(interviewPackageStorage: packagesStorage, brokenInterviewPackageStorage: brokenPackagesStorage);
            hasPackagesByInterviewId = interviewPackagesService.HasPendingPackageByInterview(interviewId);
        }

        [Test]
        public void should_return_true() => hasPackagesByInterviewId.Should().Be(true);

        private static bool hasPackagesByInterviewId;
        private static readonly Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}
