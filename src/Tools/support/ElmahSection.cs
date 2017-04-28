using System.Configuration;

namespace support
{
    public class ElmahSection : ConfigurationElement
    {
        private const string TypeAttributeName = "type";
        private const string LogPathAttributeName = "logPath";

        [ConfigurationProperty(TypeAttributeName)]
        public string Type
        {
            get
            {
                return (string)this[TypeAttributeName];
            }
            set
            {
                this[TypeAttributeName] = value;
            }
        }

        [ConfigurationProperty(LogPathAttributeName)]
        public string RelativeLogPath
        {
            get
            {
                return (string)this[LogPathAttributeName];
            }
            set
            {
                this[LogPathAttributeName] = value;
            }
        }
    }
}