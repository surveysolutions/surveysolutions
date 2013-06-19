namespace WB.Core.Infrastructure.ReadSide
{
    public interface IReadSideStatusService
    {
        bool AreViewsBeingRebuiltNow();
    }
}