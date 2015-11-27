using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.SharedKernels.DataCollection.Events.User
{
    public class UserLinkedToDevice : IEvent
    {
        public string DeviceId { get; set; }
    }
}