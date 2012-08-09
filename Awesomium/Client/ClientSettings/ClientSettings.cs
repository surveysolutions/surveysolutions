using System;

namespace Client.ClientSettings
{
    public class ClientSettings
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
