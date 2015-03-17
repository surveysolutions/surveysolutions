using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    public class PostgresReadSideCleaner : IReadSideCleaner
    {
        public void ReCreateViewDatabase()
        {
            // TODO: Implement in  KP-5071
        }

        public void CreateIndexesAfterRebuildReadSide()
        {
            // TODO: Implement in  KP-5071
        }
    }
}