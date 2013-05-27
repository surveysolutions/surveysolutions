namespace WB.Core.Infrastructure.Implementation
{
    internal class ReadLayerService : IReadLayerStatusService
    {
        public bool AreViewsBeingRebuiltNow()
        {
            return false;
        }
    }
}