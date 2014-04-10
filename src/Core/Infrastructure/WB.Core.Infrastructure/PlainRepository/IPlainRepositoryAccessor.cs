namespace WB.Core.Infrastructure.PlainRepository
{
    public interface IPlainRepositoryAccessor<TEntity>
    {
        TEntity GetById(string id);

        void Remove(string id);

        void Store(string id, TEntity entity);
    }
}