using System;
using Ncqrs.Commanding;

namespace WB.Core.Synchronization.Commands
{
    public class RegisterTabletCommand  : CommandBase
    {
        public Guid DeviceId { get; set; }

        public Guid UserId { get; set; }

        public string AppVersion { get; set; }

        public string AndroidId { get; set; }
    
        public RegisterTabletCommand(Guid deviceId, Guid userId, string appVersion, string androidId)
        {
            DeviceId = deviceId;
            UserId = userId;
            AppVersion = appVersion;
            AndroidId = androidId;
        }
    }
}