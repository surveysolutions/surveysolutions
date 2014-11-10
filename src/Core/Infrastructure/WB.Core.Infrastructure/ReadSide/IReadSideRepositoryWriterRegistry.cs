using System.Collections.Generic;

namespace WB.Core.Infrastructure.ReadSide
{
    /// <summary>
    /// Registry which contains references to all Raven-specific read side repository writers currently instantiated in application.
    /// </summary>
    public interface IReadSideRepositoryWriterRegistry
    {
        void Register(IReadSideRepositoryWriter writer);

        IEnumerable<IReadSideRepositoryWriter> GetAll();
    }
}