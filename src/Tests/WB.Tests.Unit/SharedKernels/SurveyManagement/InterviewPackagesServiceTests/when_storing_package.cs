using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    internal class when_storing_package : InterviewPackagesServiceTestsContext
    {
        Establish context = () =>
        {
            mockOfPackagesStorage = new Mock<IPlainStorageAccessor<InterviewPackage>>();

            var compressor = Mock.Of<IArchiveUtils>(x => x.CompressString(expectedPackage.Events) == compressedEvents);

            interviewPackagesService = CreateInterviewPackagesService(interviewPackageStorage: mockOfPackagesStorage.Object,
                    archiver: compressor);
        };

        Because of = () => interviewPackagesService.StorePackage(new InterviewPackage
        {
            InterviewId = expectedPackage.InterviewId,
            QuestionnaireId = expectedPackage.QuestionnaireId,
            QuestionnaireVersion = expectedPackage.QuestionnaireVersion,
            ResponsibleId = expectedPackage.ResponsibleId,
            InterviewStatus = expectedPackage.InterviewStatus,
            IsCensusInterview = expectedPackage.IsCensusInterview,
            Events = expectedPackage.Events
        });

        It should_store_specified_package =
            () => mockOfPackagesStorage.Verify(x => x.Store(Moq.It.IsAny<InterviewPackage>(), null), Times.Once);

        private static readonly InterviewPackage expectedPackage = new InterviewPackage
        {
            InterviewId = Guid.Parse("11111111111111111111111111111111"),
            QuestionnaireId = Guid.Parse("22222222222222222222222222222222"),
            ResponsibleId = Guid.Parse("33333333333333333333333333333333"),
            QuestionnaireVersion = 111,
            InterviewStatus = InterviewStatus.Restarted,
            IsCensusInterview = true,
            Events = "compressed events by interview"
        };

        private static string compressedEvents = "compressed events";
        private static InterviewPackagesService interviewPackagesService;
        private static Mock<IPlainStorageAccessor<InterviewPackage>> mockOfPackagesStorage;
    }
}