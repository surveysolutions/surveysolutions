namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public interface IReadSideCheckerAndCleaner
    {
        void ReCreateViewDatabase();
        void CreateIndexesAfterRebuildReadSide();
        bool CheckDatabaseConnection();
    }
}
