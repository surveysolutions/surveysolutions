using System;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Plugins.Messenger;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit
{
    internal static class Stub<T>
        where T : class
    {
        public static T WithNotEmptyValues
        {
            get { return new Mock<T> { DefaultValue = DefaultValue.Mock }.Object; }
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
            var sideBarSectionViewModel = new SideBarSectionViewModel(Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                Create.SubstitutionService(),
                Create.LiteEventRegistry(),
                Stub.MvxMainThreadDispatcher(),
                Mock.Of < ISideBarSectionViewModelsFactory>(),
                Mock.Of<IMvxMessenger>());
            sideBarSectionViewModel.NavigationState = Create.NavigationState();
            sideBarSectionViewModelsFactory.SetReturnsDefault(sideBarSectionViewModel);
            return sideBarSectionViewModelsFactory.Object;
        }
    }
}