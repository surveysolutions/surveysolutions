using System;
using Machine.Specifications;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    internal class when_getting_has_packages_by_interview_id : InterviewPackagesServiceTestsContext
    {
        Establish context = () =>
        {
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();

            packagesStorage.Store(new InterviewPackage {InterviewId = interviewId}, null);
            
            interviewPackagesService = CreateInterviewPackagesService(interviewPackageStorage: packagesStorage, brokenInterviewPackageStorage: brokenPackagesStorage);
        };

        Because of = () => hasPackagesByInterviewId = interviewPackagesService.HasPackagesByInterviewId(interviewId);

        It should_packages_storage_contains_packages_by_interview_id = () => hasPackagesByInterviewId.ShouldEqual(true);

        private static bool hasPackagesByInterviewId;
        private static readonly Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}