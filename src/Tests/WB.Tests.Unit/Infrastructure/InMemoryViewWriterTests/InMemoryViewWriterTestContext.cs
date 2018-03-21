using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Infrastructure.InMemoryViewWriterTests
{
    [NUnit.Framework.TestOf(typeof(InMemoryViewWriter<>))]
    internal class InMemoryViewWriterTestContext
    {
        protected static InMemoryViewWriter<T> CreateInMemoryViewWriter<T>(IReadSideRepositoryWriter<T> readSideRepositoryWriter = null,
            Guid? viewId = null) where T : class,
                IReadSideRepositoryEntity
        {
            return new InMemoryViewWriter<T>(readSideRepositoryWriter ?? Mock.Of<IReadSideRepositoryWriter<T>>(), viewId ?? Guid.NewGuid());
        }
    }
}
