using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.InterviewsSynchronizerTests.Pull
{
    [Subject(typeof (InterviewsSynchronizer))]
    [Ignore("blah")]
    public class when_interview_is_unassigned_after_assignment
    {
        Establish context = () =>
        {
            supervisorId = Guid.Parse("11111111111111111111111111111111");
            interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");



            var plainStorage = new TestPlainStorage<LocalInterviewFeedEntry>();

            plainStorage.Store(new LocalInterviewFeedEntry {
                    EntryType = EntryType.SupervisorAssigned,
                    InterviewId = interviewId.FormatGuid(),
                    SupervisorId = supervisorId.FormatGuid(),
                    Processed = false,
                    Timestamp = new DateTime(2010, 10, 1)
                }, "1");
            plainStorage.Store(new LocalInterviewFeedEntry {
                    EntryType = EntryType.InterviewUnassigned,
                    InterviewId = interviewId.FormatGuid(),
                    SupervisorId = supervisorId.FormatGuid(),
                    Processed = false,
                    Timestamp = new DateTime(2010, 10, 1)
                },"2");

            commandServiceMock = new Mock<ICommandService>();

            synchronizer = Create.InterviewsSynchronizer(plainStorage: plainStorage,
                commandService: commandServiceMock.Object);
        };

        Because of = () => synchronizer.PullInterviewsForSupervisors(new[] { supervisorId });

        It should_should_not_create_it = () => 
            commandServiceMock.Verify(x => x.Execute(it.IsAny<SynchronizeInterviewFromHeadquarters>(), Constants.HeadquartersSynchronizationOrigin), Times.Never);

        It should_unassign_interview = () => 
            commandServiceMock.Verify(x => x.Execute(it.IsAny<CancelInterviewByHQSynchronizationCommand>(), Constants.HeadquartersSynchronizationOrigin), Times.Once);
        
        private static InterviewsSynchronizer synchronizer;
        private static Guid supervisorId;
        private static Guid interviewId;
        private static Mock<ICommandService> commandServiceMock;
    }
}

