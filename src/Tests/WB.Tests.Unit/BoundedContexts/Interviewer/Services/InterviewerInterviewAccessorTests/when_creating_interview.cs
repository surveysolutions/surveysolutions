using System;
using System.Threading;
using Machine.Specifications;
using Moq;
using Newtonsoft.Json;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerInterviewAccessorTests
{
    internal class when_creating_interview
    {
        Establish context = () =>
        {
            var principal = Mock.Of<IInterviewerPrincipal>(x =>
                x.CurrentUserIdentity == Mock.Of<IInterviewerUserIdentity>(y => y.UserId == Guid.Parse("22222222222222222222222222222222")));
            var synchronizationSerializer = Mock.Of<IJsonAllTypesSerializer>(
                x => x.Deserialize<InterviewSynchronizationDto>(Moq.It.IsAny<string>()) == new InterviewSynchronizationDto());

            var quetstionnaireRepository = Mock.Of<IPlainStorage<QuestionnaireView>>(x=>x.GetById(questionnaireId.ToString()) == questionnaireInfo);

            interviewerInterviewAccessor = Create.Service.InterviewerInterviewAccessor(
                commandService: mockOfCommandService.Object,
                questionnaireRepository: quetstionnaireRepository,
                principal: principal,
                synchronizationSerializer: synchronizationSerializer);
        };

        Because of = () => interviewerInterviewAccessor.CreateInterviewAsync(interviewInfo, new InterviewerInterviewApiView()).WaitAndUnwrapException();

        It should_execute_CreateInterviewFromSynchronizationMetadata_command = () =>
            mockOfCommandService.Verify(x => x.ExecuteAsync(Moq.It.IsAny<CreateInterviewFromSynchronizationMetadata>(), null, Moq.It.IsAny<CancellationToken>()), Times.Once);

        It should_execute_InterviewSynchronizationDto_command = () =>
            mockOfCommandService.Verify(x => x.ExecuteAsync(Moq.It.IsAny<SynchronizeInterviewCommand>(), null, Moq.It.IsAny<CancellationToken>()), Times.Once);

        private static readonly QuestionnaireIdentity questionnaireId = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static InterviewerInterviewAccessor interviewerInterviewAccessor;

        private static readonly QuestionnaireView questionnaireInfo = new QuestionnaireView
        {
            Id = questionnaireId.ToString(),
            Census = false
        };
        private static readonly InterviewApiView interviewInfo = new InterviewApiView
        {
            QuestionnaireIdentity = questionnaireId,
            IsRejected = false,
        };
    }
}
