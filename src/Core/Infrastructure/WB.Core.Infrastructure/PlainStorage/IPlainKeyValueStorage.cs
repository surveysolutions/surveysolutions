#nullable enable
namespace WB.Core.Infrastructure.PlainStorage
{
    public interface IPlainKeyValueStorage<TEntity> : IPlainKeyValueStorage<TEntity, string> where TEntity : class
    {

    }

    public interface IPlainKeyValueStorage<TEntity, in TKey> where TKey: notnull where TEntity: class
    {
        TEntity? GetById(TKey id);

        bool HasNotEmptyValue(TKey id);

        void Remove(TKey id);

        void Store(TEntity entity, TKey id);
    }
}
