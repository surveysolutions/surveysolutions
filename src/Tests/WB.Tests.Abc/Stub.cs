using System;
using Moq;
using MvvmCross.Platform.Core;
using MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Tests.Abc.Storage;
using WB.Tests.Abc.TestFactories;
using WB.Core.Infrastructure.Implementation.Aggregates;

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

    // Geniuos way to override singleton.
    public class MxvMainThreadStub : MvxSingleton<IMvxMainThreadDispatcher>, IMvxMainThreadDispatcher
    {
        public bool RequestMainThreadAction(Action action, bool maskExceptions = true)
        {
            action();
            return true;
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
            return mainThreadStub.Value;
        }

        private static readonly Lazy<MxvMainThreadStub> mainThreadStub = new Lazy<MxvMainThreadStub>(() => new MxvMainThreadStub());

        public static IMvxMainThreadDispatcher InitMvxMainThreadDispatcher()
        {
            // The only way to provide own dispatcher realization, as it's not registered in mvvccross own IoC
            return mainThreadStub.Value;
        }

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

            return Mock.Of<IInterviewExpressionStatePrototypeProvider>(_ => 
                _.GetExpressionState(It.IsAny<Guid>(), It.IsAny<long>()) == expressionState);
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