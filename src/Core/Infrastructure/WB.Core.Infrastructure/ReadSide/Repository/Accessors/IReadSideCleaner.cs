namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public interface IReadSideCleaner
    {
        void ReCreateViewDatabase();
        void CreateIndexesAfterRebuildReadSide();
    }
}
