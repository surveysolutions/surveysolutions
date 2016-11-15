using System;
using MvvmCross.Platform.Core;
using Moq;
using MvvmCross.Plugins.Messenger;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Tests.Unit.SharedKernels.DataCollection;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit
{
    internal static class Stub<TInterface>
        where TInterface : class
    {
        public static TInterface WithNotEmptyValues => new Mock<TInterface> { DefaultValue = DefaultValue.Mock }.Object;

        public static TInterface Returning<TValue>(TValue value)
        {
            var mock = new Mock<TInterface>();
            mock.SetReturnsDefault(value);
            return mock.Object;
        }
    }

    internal class Stub
    {
        public static TestInMemoryWriter<TEntity> ReadSideRepository<TEntity>() where TEntity : class, IReadSideRepositoryEntity
        {
            return new TestInMemoryWriter<TEntity>();
        }

        public static IMvxMainThreadDispatcher MvxMainThreadDispatcher()
        {
            var dispatcherMock = new Mock<IMvxMainThreadDispatcher>();

            dispatcherMock
                .Setup(_ => _.RequestMainThreadAction(It.IsAny<Action>()))
                .Callback<Action>(action => action.Invoke());

            return dispatcherMock.Object;
        }

        public static ISideBarSectionViewModelsFactory SideBarSectionViewModelsFactory()
        {
            var sideBarSectionViewModelsFactory = new Mock<ISideBarSectionViewModelsFactory>();
            var sideBarSectionViewModel = new SideBarSectionViewModel(
                Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of < ISideBarSectionViewModelsFactory>(),
                Mock.Of<IMvxMessenger>(),
                Create.ViewModel.DynamicTextViewModel());
            sideBarSectionViewModel.NavigationState = Create.Other.NavigationState();
            sideBarSectionViewModelsFactory.SetReturnsDefault(sideBarSectionViewModel);
            return sideBarSectionViewModelsFactory.Object;
        }

        public static IInterviewExpressionStatePrototypeProvider InterviewExpressionStateProvider()
        {
            var expressionState = new InterviewExpressionStateStub();

            return Mock.Of<IInterviewExpressionStatePrototypeProvider>(_ => 
                _.GetExpressionState(It.IsAny<Guid>(), It.IsAny<long>()) == expressionState);
        }
    }
}