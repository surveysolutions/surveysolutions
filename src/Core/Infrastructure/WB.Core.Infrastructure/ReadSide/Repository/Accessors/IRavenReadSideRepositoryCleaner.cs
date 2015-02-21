namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public interface IRavenReadSideRepositoryCleaner
    {
        void ReCreateViewDatabase();
        void CreateIndexesAfterRebuildReadSide();
    }
}
