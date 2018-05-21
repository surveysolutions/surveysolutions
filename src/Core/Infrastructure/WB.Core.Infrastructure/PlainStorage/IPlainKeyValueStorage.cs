
namespace WB.Core.Infrastructure.PlainStorage
{
    public interface IPlainKeyValueStorage<TEntity> : IPlainKeyValueStorage<TEntity, string>
    {

    }

    public interface IPlainKeyValueStorage<TEntity, in TKey>
    {
        TEntity GetById(TKey id);

        void Remove(TKey id);

        void Store(TEntity entity, TKey id);
    }
}
