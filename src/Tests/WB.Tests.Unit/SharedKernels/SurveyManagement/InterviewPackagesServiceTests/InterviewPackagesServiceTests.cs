using System;
using System.Threading;
using Machine.Specifications;
using Main.Core.Events;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    [TestOf(typeof(InterviewPackagesService))]
    internal class InterviewPackagesServiceTests
    {
        protected static InterviewPackagesService CreateInterviewPackagesService(
            IJsonAllTypesSerializer serializer = null,
            IArchiveUtils archiver = null,
            IPlainStorageAccessor<InterviewPackage> interviewPackageStorage = null,
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage = null,
            ICommandService commandService = null,
            SyncSettings syncSettings = null)
        {
            return new InterviewPackagesService(
                syncSettings: syncSettings ?? Mock.Of<SyncSettings>(),
                logger: Mock.Of<ILogger>(),
                serializer: serializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                interviewPackageStorage: interviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<InterviewPackage>>(),
                brokenInterviewPackageStorage: brokenInterviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<BrokenInterviewPackage>>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                uniqueKeyGenerator: Mock.Of<IInterviewUniqueKeyGenerator>(),
                interviews: new TestInMemoryWriter<InterviewSummary>());
        }

        [Test]
        public void When_processing_package_and_questionnaire_does_not_exist_Than_broken_package_should_not_be_added()
        {
            //arrange
            string decompressedEvents = "decompressed events";

            var serializer =
                Mock.Of<IJsonAllTypesSerializer>(x => x.Deserialize<SyncItem>(Moq.It.IsAny<string>()) == new SyncItem() &&
                                                      x.Deserialize<InterviewMetaInfo>(Moq.It.IsAny<string>()) == new InterviewMetaInfo { Status = 0 } &&
                                                      x.Deserialize<AggregateRootEvent[]>(decompressedEvents) == new AggregateRootEvent[0]);
            var syncSettings = Mock.Of<SyncSettings>(x => x.UseBackgroundJobForProcessingPackages == false);

            var brokenPackagesStorage = new Mock<TestPlainStorage<BrokenInterviewPackage>>();

            var mockOfCommandService = new Mock<ICommandService>();
            mockOfCommandService.Setup(x => x.Execute(Moq.It.IsAny<SynchronizeInterviewEventsCommand>(),
                Moq.It.IsAny<string>())).Throws(new InterviewException("Questionnaire deleted", null, InterviewDomainExceptionType.QuestionnaireIsMissing));

            var interviewPackagesService = CreateInterviewPackagesService(
                serializer: serializer, 
                brokenInterviewPackageStorage: brokenPackagesStorage.Object,
                commandService: mockOfCommandService.Object,
                syncSettings: syncSettings);

            //act
            interviewPackagesService.StoreOrProcessPackage(new InterviewPackage
            {
                InterviewId = Guid.Parse("11111111111111111111111111111111"),
                QuestionnaireId = Guid.Parse("22222222222222222222222222222222"),
                QuestionnaireVersion = 111,
                ResponsibleId = Guid.Parse("33333333333333333333333333333333"),
                InterviewStatus = InterviewStatus.Restarted,
                IsCensusInterview = false,
                Events = "compressed serialized events"
            });

            //assert
            brokenPackagesStorage.Verify(x => x.Store(Moq.It.IsAny<BrokenInterviewPackage>(), null), Times.Never);
        }
    }
}