﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.InterviewsSynchronizerTests
{
    internal class when_pulling_rejected_by_hq_interview_created_on_client_and_interview_were_present_before_at_supervisor : InterviewsSynchronizerTestsContext
    {
        private Establish context = () =>
        {
            userDocumentStorageMock.Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<UserDocument>, IQueryable<UserDocument>>>()))
                .Returns(new[] { new UserDocument() }.AsQueryable());

            plainStorageMock.Setup(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<LocalInterviewFeedEntry>, IQueryable<LocalInterviewFeedEntry>>>())).
                Returns(
                    new[]
                    {
                        new LocalInterviewFeedEntry()
                        {
                            EntryType = EntryType.InterviewRejected,
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

            interviewSummaryStorageMock.Setup(x => x.GetById(interviewId.FormatGuid())).Returns(new InterviewSummary());

            interviewsSynchronizer = Create.InterviewsSynchronizer(interviewSummaryRepositoryWriter: interviewSummaryStorageMock.Object,
                commandService: commandServiceMock.Object, userDocumentStorage: userDocumentStorageMock.Object, plainStorage: plainStorageMock.Object, headquartersInterviewReader: headquartersInterviewReaderMock.Object);
        };

        Because of = () =>
            interviewsSynchronizer.Pull();


        It should_CreateInterviewCreatedOnClientCommand_be_never_called = () =>
            commandServiceMock.Verify(
                x =>
                    x.Execute(
                        Moq.It.IsAny<CreateInterviewCreatedOnClientCommand>(), Constants.HeadquartersSynchronizationOrigin), Times.Never);

        It should_RejectInterviewFromHeadquartersCommand_be_called_once = () =>
            commandServiceMock.Verify(
                x =>
                    x.Execute(
                        Moq.It.Is<RejectInterviewFromHeadquartersCommand>(
                            _ =>
                                _.InterviewDto == iInterviewSynchronizationDto && _.InterviewerId == userId &&
                                    _.SupervisorId == supervisorId), Constants.HeadquartersSynchronizationOrigin), Times.Once);


        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        private static Guid supervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB");

        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAc");
        private static InterviewSynchronizationDto iInterviewSynchronizationDto;


        private static Mock<IQueryableReadSideRepositoryReader<UserDocument>> userDocumentStorageMock = new Mock<IQueryableReadSideRepositoryReader<UserDocument>>();
        private static Mock<IQueryablePlainStorageAccessor<LocalInterviewFeedEntry>> plainStorageMock = new Mock<IQueryablePlainStorageAccessor<LocalInterviewFeedEntry>>();
        private static Mock<IHeadquartersInterviewReader> headquartersInterviewReaderMock = new Mock<IHeadquartersInterviewReader>();
        private static Mock<IReadSideRepositoryWriter<InterviewSummary>> interviewSummaryStorageMock = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
    }
}
