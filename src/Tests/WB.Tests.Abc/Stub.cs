using System;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Tests.Abc.Storage;
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

    internal class Stub
    {
        public static TestInMemoryWriter<TEntity> ReadSideRepository<TEntity>() where TEntity : class, IReadSideRepositoryEntity
        {
            return new TestInMemoryWriter<TEntity>();
        }

        public static IMvxMainThreadAsyncDispatcher MvxMainThreadAsyncDispatcher() =>
            Create.Fake.MvxMainThreadAsyncDispatcher();

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
