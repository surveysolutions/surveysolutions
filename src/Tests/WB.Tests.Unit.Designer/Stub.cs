using System;
using MvvmCross.Platform.Core;
using Moq;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit
{
    internal static class Stub<T>
        where T : class
    {
        public static T WithNotEmptyValues => new Mock<T> { DefaultValue = DefaultValue.Mock }.Object;
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
    }
}