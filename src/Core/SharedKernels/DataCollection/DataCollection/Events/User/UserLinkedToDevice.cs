using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.SharedKernels.DataCollection.Events.User
{
    public class UserLinkedToDevice : ILiteEvent
    {
        public string DeviceId { get; set; }
    }
}