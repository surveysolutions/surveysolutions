using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    public class LinkUserToDevice : CommandBase
    {
        public LinkUserToDevice(Guid id, string deviceId)
        {
            this.Id = id;
            this.DeviceId = deviceId;
        }

        public Guid Id { get; set; }

        public string DeviceId { get; set; }
    }
}