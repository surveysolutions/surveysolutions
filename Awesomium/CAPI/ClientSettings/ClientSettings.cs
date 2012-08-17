using System;
using Synchronization.Core.Interface;

namespace Browsing.CAPI.ClientSettings
{
    public class ClientSettings : ISettings
    {
        public ClientSettings(Guid parentId, Guid clientId)
        {
            ParentId = parentId;
            ClientId = clientId;
        }

        public Guid ClientId { get; set; }
        public Guid ParentId { get; set; }
    }
}
