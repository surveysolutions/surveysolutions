﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;

using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Synchronization.InterviewsSynchronizerTests.Pull
{
    internal class when_pulling_new_interview_created_by_deleted_questionnaire : InterviewsSynchronizerTestsContext
    {
        private Establish context = () =>
        {
            userDocumentStorageMock.Setup(x => x.GetById(Moq.It.IsAny<string>()))
            .Returns(new UserDocument());

            plainStorageMock.Setup(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<LocalInterviewFeedEntry>, List<LocalInterviewFeedEntry>>>())).
                Returns(
                    new[]
                    {
                        new LocalInterviewFeedEntry()
                        {
                            EntryType = EntryType.SupervisorAssigned,
                            UserId = userId.FormatGuid(),
                            SupervisorId = supervisorId.FormatGuid(),
                            InterviewId = interviewId.FormatGuid(),
                            InterviewerId = userId.FormatGuid()
                        }
                    }.ToList());

            iInterviewSynchronizationDto = Create.Other.InterviewSynchronizationDto(status: InterviewStatus.SupervisorAssigned,
                    userId: userId,
                    questionnaireId: questionnaireId,
                    questionnaireVersion: 2,
                    wasCompleted: true,
                    interviewId: interviewId);

            headquartersInterviewReaderMock.Setup(x => x.GetInterviewByUri(Moq.It.IsAny<Uri>()))
                .Returns(
                    Task.FromResult(iInterviewSynchronizationDto));

            interviewsSynchronizer = Create.Other.InterviewsSynchronizer(
                commandService: commandServiceMock.Object, userDocumentStorage: userDocumentStorageMock.Object,
                plainStorage: plainStorageMock.Object, headquartersInterviewReader: headquartersInterviewReaderMock.Object,
                plainQuestionnaireRepository:
                    Mock.Of<IPlainQuestionnaireRepository>());
        };

        Because of = () =>
            interviewsSynchronizer.PullInterviewsForSupervisors(new[] { Guid.NewGuid() });


        It should_not_be_called_SynchronizeInterviewFromHeadquarters = () =>
            commandServiceMock.Verify(
                x =>
                    x.Execute(
                        Moq.It.Is<SynchronizeInterviewFromHeadquarters>(
                            _ =>
                                _.Id == interviewId && _.UserId == userId && _.InterviewDto == iInterviewSynchronizationDto), Constants.HeadquartersSynchronizationOrigin), Times.Never);



        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        private static Guid supervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB");

        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAc");
        private static InterviewSynchronizationDto iInterviewSynchronizationDto;

        private static Mock<IPlainStorageAccessor<UserDocument>> userDocumentStorageMock = new Mock<IPlainStorageAccessor<UserDocument>>();
        private static Mock<IPlainStorageAccessor<LocalInterviewFeedEntry>> plainStorageMock = new Mock<IPlainStorageAccessor<LocalInterviewFeedEntry>>();
        private static Mock<IHeadquartersInterviewReader> headquartersInterviewReaderMock = new Mock<IHeadquartersInterviewReader>();
    }
}
