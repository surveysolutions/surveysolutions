using System;
using System.Configuration;
using WB.Infrastructure.Native;

namespace WB.UI.Shared.Web.Slack
{
    public class SlackChannelConfiguration : ConfigurationElement
    {
        private const string name = "name";
        private const string type = "type";

        [ConfigurationProperty(type, IsRequired = true, IsKey = true)]
        public FatalExceptionType Type
        {
            get
            {
                if (Enum.TryParse<FatalExceptionType>(base[type].ToString(), out var exceptionType)) return exceptionType;

                return FatalExceptionType.None;
            }
            //private set => base[type] = value.ToString();
        }

        [ConfigurationProperty(name, IsRequired = true)]
        public string Name
        {
            get => base[name].ToString();
            //private set => base[name] = value;
        }
    }
}
