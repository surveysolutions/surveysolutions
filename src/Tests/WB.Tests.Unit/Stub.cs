using System;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Plugins.Messenger;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
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
            sideBarSectionViewModelsFactory.SetReturnsDefault(new SideBarSectionViewModel(Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                Create.SubstitutionService(),
                Create.LiteEventRegistry(),
                Stub.MvxMainThreadDispatcher(),
                Mock.Of < ISideBarSectionViewModelsFactory>(),
                Mock.Of<IMvxMessenger>()));
            return sideBarSectionViewModelsFactory.Object;
        }
    }
}