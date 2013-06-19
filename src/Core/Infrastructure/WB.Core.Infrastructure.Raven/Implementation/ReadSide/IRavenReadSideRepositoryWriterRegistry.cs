using System.Collections.Generic;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide
{
    /// <summary>
    /// Registry which contains references to all Raven-specific read side repository writers currently instantiated in application.
    /// </summary>
    internal interface IRavenReadSideRepositoryWriterRegistry
    {
        void Register(IRavenReadSideRepositoryWriter writer);

        IEnumerable<IRavenReadSideRepositoryWriter> GetAll();
    }
}