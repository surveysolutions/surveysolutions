using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.InMemoryViewWriterTests
{
    [Subject(typeof(InMemoryViewWriter<>))]
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
