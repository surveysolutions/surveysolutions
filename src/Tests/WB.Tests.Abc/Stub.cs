using System;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Tests.Abc.Storage;
using WB.Tests.Abc.TestFactories;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Abc
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

        public static IMvxMainThreadDispatcher MvxMainThreadDispatcher() => Create.Fake.MvxMainThreadDispatcher();

        public static ISideBarSectionViewModelsFactory SideBarSectionViewModelsFactory()
        {
            var liteEventRegistry = Create.Service.LiteEventRegistry();
            var sideBarSectionViewModelsFactory = new Mock<ISideBarSectionViewModelsFactory>();
            var sideBarSectionViewModel = new SideBarSectionViewModel(
                Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IMvxMessenger>(),
                liteEventRegistry,
                Create.ViewModel.DynamicTextViewModel(liteEventRegistry),
                Create.Entity.AnswerNotifier(liteEventRegistry));
            sideBarSectionViewModel.NavigationState = Create.Other.NavigationState();
            sideBarSectionViewModelsFactory.SetReturnsDefault(sideBarSectionViewModel);
            return sideBarSectionViewModelsFactory.Object;
        }

        public static IInterviewExpressionStatePrototypeProvider InterviewExpressionStateProvider()
        {
            var expressionState = new InterviewExpressionStateStub();
            var storage = new InterviewExpressionStorageStub();
            return Mock.Of<IInterviewExpressionStatePrototypeProvider>(_ 
                => _.GetExpressionState(It.IsAny<Guid>(), It.IsAny<long>()) == expressionState
                && _.GetExpressionStorage(Moq.It.IsAny<QuestionnaireIdentity>()) == storage);
        }

        internal class StubAggregateLock : IAggregateLock
        {
            public T RunWithLock<T>(string aggregateGuid, Func<T> run)
            {
                return run();
            }

            public void RunWithLock(string aggregateGuid, Action run)
            {
                run();
            }
        }

        static IAggregateLock stubAggregateLock = new StubAggregateLock();

        public static IAggregateLock Lock() => stubAggregateLock;
    }
}
