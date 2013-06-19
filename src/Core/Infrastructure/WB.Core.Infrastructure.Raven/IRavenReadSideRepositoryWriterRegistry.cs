using WB.Core.Infrastructure.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.Infrastructure.Raven
{
    /// <summary>
    /// Registry which contains references to all Raven-specific read side repository writers currently instantiated in application.
    /// </summary>
    internal interface IRavenReadSideRepositoryWriterRegistry
    {
        void Register(IRavenReadSideRepositoryWriter writer);
    }
}