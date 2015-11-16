namespace WB.Core.Infrastructure.ReadSide
{
    public interface IReadSideStatusService
    {
        bool AreViewsBeingRebuiltNow();
        bool IsReadSideOutdated();

        int GetReadSideApplicationVersion();
        int? GetReadSideDatabaseVersion();
    }
}