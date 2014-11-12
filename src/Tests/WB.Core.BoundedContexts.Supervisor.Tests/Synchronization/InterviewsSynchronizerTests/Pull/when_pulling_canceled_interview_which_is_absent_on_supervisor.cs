﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding.ServiceModel;
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

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.InterviewsSynchronizerTests.Pull
{
    internal class when_pulling_canceled_interview_which_is_absent_on_supervisor : InterviewsSynchronizerTestsContext
    {
        private Establish context = () =>
        {
            plainStorageMock.Setup(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<LocalInterviewFeedEntry>, IQueryable<LocalInterviewFeedEntry>>>())).
                Returns(
                    new[]
                    {
                        new LocalInterviewFeedEntry()
                        {
                            EntryType = EntryType.InterviewUnassigned,
                            UserId = userId.FormatGuid(),
                            SupervisorId = supervisorId.FormatGuid(),
                            InterviewId = interviewId.FormatGuid(),
                            InterviewerId = userId.FormatGuid()
                        }
                    }.AsQueryable());

            iInterviewSynchronizationDto = new InterviewSynchronizationDto(interviewId, InterviewStatus.Deleted, "",
                        userId, questionnaireId, 2, new AnsweredQuestionSynchronizationDto[0], new HashSet<InterviewItemId>(),
                        new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(),
                        new Dictionary<InterviewItemId, int>(), new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(), true);

            headquartersInterviewReaderMock.Setup(x => x.GetInterviewByUri(Moq.It.IsAny<Uri>()))
                .Returns(
                    Task.FromResult(iInterviewSynchronizationDto));


            interviewsSynchronizer = Create.InterviewsSynchronizer(interviewSummaryRepositoryWriter: interviewSummaryStorageMock.Object,
                commandService: commandServiceMock.Object, userDocumentStorage: userDocumentStorageMock.Object, plainStorage: plainStorageMock.Object, headquartersInterviewReader: headquartersInterviewReaderMock.Object);
        };

        Because of = () =>
            interviewsSynchronizer.PullInterviewsForSupervisors(new []{Guid.NewGuid()});


        It should_CancelInterviewByHQSynchronizationCommand_be_never_called = () =>
            commandServiceMock.Verify(
                x =>
                    x.Execute(
                        Moq.It.IsAny<CancelInterviewByHqSynchronizationCommand>(), Constants.HeadquartersSynchronizationOrigin), Times.Never);


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
