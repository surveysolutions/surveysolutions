using System;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.EnumerationStageViewModelTests
{
    internal class when_initializing_active_group_view_model
    {
        Establish context = () =>
        {
            var interviewViewModelFactory = Mock.Of<IInterviewViewModelFactory>(f =>
                f.GetNew<GroupNavigationViewModel>() == Mock.Of<GroupNavigationViewModel>());

            interviewRepositoryMock.Setup(x => x.Get(Moq.It.IsAny<string>())).Returns(Mock.Of<IStatefulInterview>());
            enumerationStage = Create.ViewModel.EnumerationStageViewModel(
                eventRegistry: eventRegistry.Object, 
                interviewRepository: interviewRepositoryMock.Object,
                interviewViewModelFactory: interviewViewModelFactory,
                compositeCollectionInflationService: new CompositeCollectionInflationService());
        };

        Because of = () => enumerationStage.Init(interviewId, navigationState.Object, groupId, null);

        It should_subscribe_view_model_for_events =
            () => eventRegistry.Verify(x => x.Subscribe(enumerationStage, interviewId), Times.Once);

        static EnumerationStageViewModel enumerationStage;

        static readonly string interviewId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        static readonly Identity groupId = new Identity(Guid.NewGuid(), new decimal[0]);

        static readonly Mock<NavigationState> navigationState = new Mock<NavigationState>();

        static readonly Mock<ILiteEventRegistry> eventRegistry = new Mock<ILiteEventRegistry>();
        static readonly Mock<IStatefulInterviewRepository> interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();
    }
}