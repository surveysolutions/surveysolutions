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

        public string BaseAddress()
        {
            return this.applicationSettings.GetPathToDesigner();
        }

        public TimeSpan GetTimeout()
        {
            return this.applicationSettings.GetHttpTimeout();
        }
    }
}