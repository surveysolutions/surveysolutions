using System;
using Ncqrs.Commanding;

namespace Main.Core.Commands.Sync
{
    [Serializable]
    public class CreateSyncActivityCommand : CommandBase
    {
        public Guid publicKey { get; set; }

        public Guid ClientDeviceId { get; set; }
    
        public CreateSyncActivityCommand(Guid id, Guid deviceId)
        {
            this.publicKey = id;
            this.ClientDeviceId = deviceId;
        }
    }
}
