using System;
using Synchronization.Core.Interface;

namespace Browsing.Supervisor.ClientSettings
{
    public class ClientSettings : ISettings
    {

        public Guid ClientId { get; set; }
        public Guid ParentId { get; set; }

        public ClientSettings(Guid parentId, Guid clientId)
        {
            ParentId = parentId;
            ClientId = clientId;
        }
    }
}
