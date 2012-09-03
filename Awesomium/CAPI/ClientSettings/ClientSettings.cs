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

        public bool RunEngine
        {
            get { return Properties.Settings.Default.RunClient; }
        }

        public string DefaultPort
        {
            get { return Properties.Settings.Default.DefaultPort; }
        }

        public string EnginePathName
        {
            get { return Properties.Settings.Default.EnginePathName; }
        }

    }
}
