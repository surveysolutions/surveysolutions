using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public interface IReadSideStorage<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        TEntity GetById(string id);

        void Remove(string id);

        void Store(TEntity view, string id);
    }
}