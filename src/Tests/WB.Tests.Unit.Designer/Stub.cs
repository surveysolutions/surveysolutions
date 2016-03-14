using Moq;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Designer
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
    }
}