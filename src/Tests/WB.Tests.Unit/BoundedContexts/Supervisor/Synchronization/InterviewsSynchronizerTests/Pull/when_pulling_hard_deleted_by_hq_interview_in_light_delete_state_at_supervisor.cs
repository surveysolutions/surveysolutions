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
    internal class when_pulling_hard_deleted_by_hq_interview_in_light_delete_state_at_supervisor : InterviewsSynchronizerTestsContext
    {
        private Establish context = () =>
        {
            userDocumentStorageMock.Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<UserDocument>, IQueryable<UserDocument>>>()))
                .Returns(new[] { new UserDocument() }.AsQueryable());

            userDocumentStorageMock.Setup(x => x.GetById(Moq.It.IsAny<string>()))
            .Returns(new UserDocument());

            plainStorageMock.Setup(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<LocalInterviewFeedEntry>, IQueryable<LocalInterviewFeedEntry>>>())).
                Returns(
                    new[]
                    {
                        new LocalInterviewFeedEntry()
                        {
                            EntryType = EntryType.InterviewDeleted,
                            UserId = userId.FormatGuid(),
                            SupervisorId = supervisorId.FormatGuid(),
                            InterviewId = interviewId.FormatGuid(),
                            InterviewerId = userId.FormatGuid()
                        }
                    }.AsQueryable());

            iInterviewSynchronizationDto = new InterviewSynchronizationDto(interviewId, InterviewStatus.RejectedByHeadquarters, "",
                        userId, questionnaireId, 2, new AnsweredQuestionSynchronizationDto[0], new HashSet<InterviewItemId>(),
                        new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(),
                        new Dictionary<InterviewItemId, int>(), new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(), true);

            headquartersInterviewReaderMock.Setup(x => x.GetInterviewByUri(Moq.It.IsAny<Uri>()))
                .Returns(
                    Task.FromResult(iInterviewSynchronizationDto));

            var interviewSummaryStorageMock = new Mock<IReadSideRepositoryReader<InterviewSummary>>();
            interviewSummaryStorageMock.Setup(x => x.GetById(interviewId.FormatGuid())).Returns(new InterviewSummary() { IsDeleted = true });

            interviewsSynchronizer = Create.InterviewsSynchronizer(interviewSummaryRepositoryReader: interviewSummaryStorageMock.Object,
                commandService: commandServiceMock.Object, userDocumentStorage: userDocumentStorageMock.Object, plainStorage: plainStorageMock.Object, headquartersInterviewReader: headquartersInterviewReaderMock.Object);
        };

        Because of = () =>
            interviewsSynchronizer.PullInterviewsForSupervisors(new []{supervisorId});

        It should_HardDeleteInterview_be_called_once = () =>
            commandServiceMock.Verify(
                x =>
                    x.Execute(
                        Moq.It.Is<HardDeleteInterview>(
                            _ =>
                                _.InterviewId==interviewId && _.UserId==userId), Constants.HeadquartersSynchronizationOrigin), Times.Once);


        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        private static Guid supervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB");

        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAc");
        private static InterviewSynchronizationDto iInterviewSynchronizationDto;


        private static Mock<IQueryableReadSideRepositoryReader<UserDocument>> userDocumentStorageMock = new Mock<IQueryableReadSideRepositoryReader<UserDocument>>();
        private static Mock<IPlainStorageAccessor<LocalInterviewFeedEntry>> plainStorageMock = new Mock<IPlainStorageAccessor<LocalInterviewFeedEntry>>();
        private static Mock<IHeadquartersInterviewReader> headquartersInterviewReaderMock = new Mock<IHeadquartersInterviewReader>();
    }
}
