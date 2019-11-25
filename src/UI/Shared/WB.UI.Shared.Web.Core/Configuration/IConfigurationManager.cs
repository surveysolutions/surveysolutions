using System.Collections.Specialized;

namespace WB.UI.Shared.Web.Configuration
{
    public interface IConfigurationManager
    {
        NameValueCollection AppSettings { get; }
        NameValueCollection MembershipSettings { get; }
    }
}
