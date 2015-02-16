using System;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class RestServiceSettings : IRestServiceSettings
    {
        private readonly ApplicationSettings applicationSettings;

        public RestServiceSettings(ApplicationSettings applicationSettings)
        {
            this.applicationSettings = applicationSettings;
        }

        public string Endpoint
        {
            get { return this.applicationSettings.DesignerEndpoint; }
            set { this.applicationSettings.DesignerEndpoint = value; }
        }

        public TimeSpan Timeout
        {
            get { return this.applicationSettings.HttpResponseTimeout; }
            set { this.applicationSettings.HttpResponseTimeout = value; }
        }

        public int BufferSize
        {
            get { return this.applicationSettings.BufferSize; }
            set { this.applicationSettings.BufferSize = value; }
        }
    }
}