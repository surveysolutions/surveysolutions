using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    [TestFixture]
    internal class when_storing_package
    {
        [OneTimeSetUp]
        public void Setup()
        {
            mockOfPackagesStorage = new Mock<IPlainStorageAccessor<InterviewPackage>>();

            var syncSettings = Mock.Of<SyncSettings>();
            
            interviewPackagesService = Create.Service.InterviewPackagesService(interviewPackageStorage: mockOfPackagesStorage.Object, syncSettings: syncSettings);

            interviewPackagesService.StoreOrProcessPackage(new InterviewPackage
            {
                InterviewId = expectedPackage.InterviewId,
                QuestionnaireId = expectedPackage.QuestionnaireId,
                QuestionnaireVersion = expectedPackage.QuestionnaireVersion,
                ResponsibleId = expectedPackage.ResponsibleId,
                InterviewStatus = expectedPackage.InterviewStatus,
                IsCensusInterview = expectedPackage.IsCensusInterview,
                Events = expectedPackage.Events
            });
        }

        [Test]
        public void should_store_specified_package() 
            => mockOfPackagesStorage.Verify(x => x.Store(It.IsAny<InterviewPackage>(), null), Times.Once);

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

        private static InterviewPackagesService interviewPackagesService;
        private static Mock<IPlainStorageAccessor<InterviewPackage>> mockOfPackagesStorage;
    }
}
