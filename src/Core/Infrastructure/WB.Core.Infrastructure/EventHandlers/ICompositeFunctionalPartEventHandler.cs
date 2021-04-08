using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface ICompositeFunctionalPartEventHandler<TEntity, TStorage>
        where TEntity : class, IReadSideRepositoryEntity
        where TStorage : class, IReadSideStorage<TEntity>
    { }
}
