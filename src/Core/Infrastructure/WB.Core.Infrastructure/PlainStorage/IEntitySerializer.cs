namespace WB.Core.Infrastructure.PlainStorage
{
    public interface IEntitySerializer<TEntity> where TEntity: class
    {
        string Serialize(TEntity entity);

        TEntity Deserialize(string json);
    }
}