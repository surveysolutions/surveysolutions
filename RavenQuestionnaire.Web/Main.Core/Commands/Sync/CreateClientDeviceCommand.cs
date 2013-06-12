namespace Main.Core.Commands.Sync
{
    using System;
    using Main.Core.Domain;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootConstructor(typeof(ClientDeviceAR))]
    public class CreateClientDeviceCommand
    {
        public Guid Id;
        public string DeviceId;
        public Guid ClientInstanceKey;
        public string DeviceType;

        public CreateClientDeviceCommand(Guid id, string deviceId, Guid clientInstanceKey, string deviceType)
        {
            this.Id = id;
            this.DeviceId = deviceId;
            this.ClientInstanceKey = clientInstanceKey;
            this.DeviceType = deviceType;
        }
    }
}
