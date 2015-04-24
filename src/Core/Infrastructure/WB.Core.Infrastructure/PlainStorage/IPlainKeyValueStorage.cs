namespace WB.Core.Infrastructure.PlainStorage
{
    public interface IPlainKeyValueStorage<TEntity>
    {
        TEntity GetById(string id);

        void Remove(string id);

        void Store(TEntity view, string id);
    }
}