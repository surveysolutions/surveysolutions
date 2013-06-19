using System;

namespace WB.Core.Infrastructure.ReadSide
{
    /// <summary>
    /// Accessor for read-side repository which should be used to perform queries.
    /// </summary>
    public interface IReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    #warning TLK: make string identifiers here after switch to new storage
    {
        int Count();

        TEntity GetById(Guid id);
    }
}