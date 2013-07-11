namespace WB.Core.Infrastructure.ReadSide
{
    public interface IReadSideAdministrationService
    {
        string GetReadableStatus();

        void RebuildAllViewsAsync();

        void StopAllViewsRebuilding();
    }
}
