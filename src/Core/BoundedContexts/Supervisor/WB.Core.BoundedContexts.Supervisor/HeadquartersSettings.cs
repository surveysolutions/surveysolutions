using System;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class HeadquartersSettings
    {
        public Uri LoginServiceEndpointUrl { get; private set; }

        public HeadquartersSettings(Uri loginServiceEndpointUrl)
        {
            this.LoginServiceEndpointUrl = loginServiceEndpointUrl;
        }
    }
}