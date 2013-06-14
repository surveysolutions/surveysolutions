namespace WB.Core.Infrastructure.ReadSide
{
    /// <summary>
    /// Accessor for read-side repository which should be used to update repository with new data from write-side.
    /// Please note, that reader (IReadSideRepositoryReader) should not be used to get data needed for such an update.
    /// All needed querying methods are already in this interface and should be used because they might be optimized for such operations.
    /// </summary>
    public interface IReadSideRepositoryWriter<TEntity>
        where TEntity : class, IReadSideRepositoryEntity { }
}