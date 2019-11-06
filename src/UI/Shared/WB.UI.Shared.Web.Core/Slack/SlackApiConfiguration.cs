using System;
using System.Configuration;

namespace WB.UI.Shared.Web.Slack
{
    public class SlackApiConfiguration : ConfigurationSection
    {
        private const string oauthToken = "token";

        [ConfigurationProperty(oauthToken, IsRequired = false)]
        public string OuathToken
        {
            get => base[oauthToken].ToString();
            //private set => base[oauthToken] = value;
        }

        private const string throttle = "throttle";
        [ConfigurationProperty(throttle, IsRequired = false, DefaultValue = "00:01:00")]
        public TimeSpan Throttle
        {
            get => TimeSpan.Parse(base[throttle].ToString());
            //private set => base[throttle] = value.ToString();
        }

        private const string channels = "channels";
        [ConfigurationProperty(channels)]
        public ChannelsCollection Channels => (ChannelsCollection)base[channels];
    }


}
