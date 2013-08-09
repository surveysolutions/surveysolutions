﻿namespace Main.Core.Commands.Sync
{
    using System;
    using Main.Core.Domain;
    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootConstructor(typeof(ClientDeviceAR))]
    public class CreateClientDeviceCommand  : CommandBase
    {
        public Guid Id { get; set; }
    
        public string DeviceId { get; set; }
    
        public Guid ClientInstanceKey { get; set; }
    
        //public string DeviceType;

        public CreateClientDeviceCommand(Guid id, string deviceId, Guid clientInstanceKey/*, string deviceType*/)
        {
            this.Id = id;
            this.DeviceId = deviceId;
            this.ClientInstanceKey = clientInstanceKey;
            //this.DeviceType = deviceType;
        }
    }
}
