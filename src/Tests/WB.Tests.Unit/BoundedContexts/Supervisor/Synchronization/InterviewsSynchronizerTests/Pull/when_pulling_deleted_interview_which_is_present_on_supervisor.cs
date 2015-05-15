using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;

using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
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

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Synchronization.InterviewsSynchronizerTests.Pull
{
    internal class when_pulling_deleted_interview_which_is_present_on_supervisor : InterviewsSynchronizerTestsContext
    {
        private Establish context = () =>
        {
            var plainStorageMock = new TestPlainStorage<LocalInterviewFeedEntry>();
            plainStorageMock.Store(
                        new LocalInterviewFeedEntry()
                        {
                            EntryType = EntryType.InterviewDeleted,
                            UserId = userId.FormatGuid(),
                            SupervisorId = supervisorId.FormatGuid(),
                            InterviewId = interviewId.FormatGuid(),
                            InterviewerId = userId.FormatGuid(),
                            EntryId = "1"
                        }, "1");

            iInterviewSynchronizationDto = new InterviewSynchronizationDto(interviewId, InterviewStatus.Deleted, "",
                        userId, questionnaireId, 2, new AnsweredQuestionSynchronizationDto[0], new HashSet<InterviewItemId>(),
                        new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(),
                        new Dictionary<InterviewItemId, int>(), new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(), true);

            headquartersInterviewReaderMock.Setup(x => x.GetInterviewByUri(Moq.It.IsAny<Uri>()))
                .Returns(
                    Task.FromResult(iInterviewSynchronizationDto));

            interviewSummaryStorageMock.Setup(x => x.GetById(interviewId.FormatGuid())).Returns(new InterviewSummary());

            interviewsSynchronizer = Create.InterviewsSynchronizer(interviewSummaryRepositoryReader: interviewSummaryStorageMock.Object,
                commandService: commandServiceMock.Object, 
                userDocumentStorage: userDocumentStorageMock.Object, 
                plainStorage: plainStorageMock, 
                headquartersInterviewReader: headquartersInterviewReaderMock.Object);
        };

        Because of = () =>
            interviewsSynchronizer.PullInterviewsForSupervisors(new[] { supervisorId });

        It should_HardDeleteInterview_be_called_once = () =>
            commandServiceMock.Verify(
                x =>
                    x.Execute(
                        Moq.It.IsAny<HardDeleteInterview>(), Constants.HeadquartersSynchronizationOrigin), Times.Once);


        static InterviewsSynchronizer interviewsSynchronizer;
        static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid supervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB");
        static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAc");

        static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();

        static InterviewSynchronizationDto iInterviewSynchronizationDto;

        static Mock<IQueryableReadSideRepositoryReader<UserDocument>> userDocumentStorageMock = new Mock<IQueryableReadSideRepositoryReader<UserDocument>>();
        static Mock<IHeadquartersInterviewReader> headquartersInterviewReaderMock = new Mock<IHeadquartersInterviewReader>();
        static Mock<IReadSideRepositoryReader<InterviewSummary>> interviewSummaryStorageMock = new Mock<IReadSideRepositoryReader<InterviewSummary>>();
    }
}
