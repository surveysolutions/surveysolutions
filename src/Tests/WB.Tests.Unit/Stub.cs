using Moq;
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
    }
}