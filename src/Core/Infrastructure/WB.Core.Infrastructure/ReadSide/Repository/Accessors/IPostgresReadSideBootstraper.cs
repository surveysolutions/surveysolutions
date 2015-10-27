namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public interface IPostgresReadSideBootstraper
    {
        void ReCreateViewDatabase();
        void CreateIndexesAfterRebuildReadSide();
        bool CheckDatabaseConnection();
    }
}
