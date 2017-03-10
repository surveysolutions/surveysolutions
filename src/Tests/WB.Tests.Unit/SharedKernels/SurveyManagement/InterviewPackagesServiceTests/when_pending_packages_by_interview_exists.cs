using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    internal class when_pending_packages_by_interview_exists : InterviewPackagesServiceTestsContext
    {
        Establish context = () =>
        {
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();

            packagesStorage.Store(new InterviewPackage {InterviewId = interviewId}, null);
            
            interviewPackagesService = CreateInterviewPackagesService(interviewPackageStorage: packagesStorage, brokenInterviewPackageStorage: brokenPackagesStorage);
        };

        Because of = () => hasPackagesByInterviewId = interviewPackagesService.HasPendingPackageByInterview(interviewId);

        It should_return_true = () => hasPackagesByInterviewId.ShouldEqual(true);

        private static bool hasPackagesByInterviewId;
        private static readonly Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}