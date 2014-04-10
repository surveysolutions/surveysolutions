namespace WB.Core.Infrastructure.PlainStorage
{
    public interface IPlainStorageAccessor<TEntity>
        where TEntity : class
    {
        TEntity GetById(string id);

        void Remove(string id);

        void Store(TEntity entity, string id);
    }
}