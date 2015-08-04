using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Synchronization.InterviewsSynchronizerTests.Pull
{
    [Subject(typeof (InterviewsSynchronizer))]
    public class when_interview_is_unassigned_after_assignment
    {
        Establish context = () =>
        {
            var userDocumentStorageMock = new Mock<IQueryableReadSideRepositoryReader<UserDocument>>();
            userDocumentStorageMock.Setup(x => x.GetById(Moq.It.IsAny<string>()))
             .Returns(new UserDocument());

            var interviewSummaryStorage =
                Mock.Of<IReadSideRepositoryReader<InterviewSummary>>(_ => _.GetById(it.IsAny<string>()) == new InterviewSummary());

            var interviewSynchronizationDto = new InterviewSynchronizationDto(interviewId, InterviewStatus.Deleted, "",
                     userId, questionnaireId, 2, new AnsweredQuestionSynchronizationDto[0], new HashSet<InterviewItemId>(),
                     new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(),
                     new Dictionary<InterviewItemId, int>(), new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(), true);

            headquartersInterviewReaderMock.Setup(x => x.GetInterviewByUri(Moq.It.IsAny<Uri>()))
                .Returns(
                    Task.FromResult(interviewSynchronizationDto));

            var plainStorage = new TestPlainStorage<LocalInterviewFeedEntry>();

            plainStorage.Store(new LocalInterviewFeedEntry
            {
                EntryId = "EntryId1",
                EntryType = EntryType.SupervisorAssigned,
                InterviewId = interviewId.FormatGuid(),
                SupervisorId = supervisorId.FormatGuid(),
                Processed = false,
                Timestamp = new DateTime(2010, 10, 1)
            }, "1");

            plainStorage.Store(new LocalInterviewFeedEntry
            {
                EntryId = "EntryId2",
                EntryType = EntryType.InterviewUnassigned,
                InterviewId = interviewId.FormatGuid(),
                SupervisorId = supervisorId.FormatGuid(),
                UserId = userId.FormatGuid(),
                Processed = false,
                Timestamp = new DateTime(2010, 10, 2)
            }, "2");

            commandServiceMock = new Mock<ICommandService>();

            synchronizer = Create.InterviewsSynchronizer(plainStorage: plainStorage,
                commandService: commandServiceMock.Object, headquartersInterviewReader: headquartersInterviewReaderMock.Object,
                userDocumentStorage: userDocumentStorageMock.Object, interviewSummaryRepositoryReader: interviewSummaryStorage);
        };

        Because of = () => synchronizer.PullInterviewsForSupervisors(new[] { supervisorId });

        It should_should_not_create_it = () =>
            commandServiceMock.Verify(x => x.Execute(it.IsAny<SynchronizeInterviewFromHeadquarters>(), Constants.HeadquartersSynchronizationOrigin, Moq.It.IsAny<bool>()), Times.Never);

        It should_unassign_interview = () =>
            commandServiceMock.Verify(x => x.Execute(it.IsAny<CancelInterviewByHqSynchronizationCommand>(), Constants.HeadquartersSynchronizationOrigin, Moq.It.IsAny<bool>()), Times.Once);
        
        private static InterviewsSynchronizer synchronizer;
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Mock<ICommandService> commandServiceMock;
        private static Mock<IHeadquartersInterviewReader> headquartersInterviewReaderMock = new Mock<IHeadquartersInterviewReader>();

        private static Guid questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
    }
}

