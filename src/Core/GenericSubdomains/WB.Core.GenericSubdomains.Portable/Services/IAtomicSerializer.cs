namespace WB.Core.GenericSubdomains.Portable.Services
{
    public interface IAtomicSerializer<TEntity>
    {
        string Serialize(TEntity entity);

        TEntity Deserialize(string json);
    }
}
