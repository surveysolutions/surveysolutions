using System.Linq;
using System.Net;
using System.Web;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class IpAddressProvider : IIpAddressProvider
    {
        private ILogger log => ServiceLocator.Current.GetInstance<ILogger>();

        public IPAddress GetClientIpAddress()
        {
            log.Error("================= IP Address =================================");
            IPAddress ip = null;
            var userHostAddress = HttpContext.Current.Request.UserHostAddress ?? "";

            log.Error($"userHostAddress: {userHostAddress}");

            IPAddress.TryParse(userHostAddress, out ip);

            var xForwardedForKey = HttpContext.Current.Request.ServerVariables.AllKeys.FirstOrDefault(x => x.ToUpper() == "X_FORWARDED_FOR");

            var isXForwardedKeyIsMissing = string.IsNullOrEmpty(xForwardedForKey);
            if (isXForwardedKeyIsMissing)
                return ip;

            var xForwardedFor = HttpContext.Current.Request.ServerVariables[xForwardedForKey];

            log.Error($"xForwardedFor: {xForwardedFor}");

            var isForwardedParamIsEmpty = string.IsNullOrEmpty(xForwardedFor);
            if (isForwardedParamIsEmpty)
                return ip;

            return xForwardedFor.Split(',').Select(IPAddress.Parse).LastOrDefault(x => !IsPrivateIpAddress(x)) ?? ip;
        }

        private static bool IsPrivateIpAddress(IPAddress ip)
        {
            // http://en.wikipedia.org/wiki/Private_network
            // Private IP Addresses are: 
            //  24-bit block: 10.0.0.0 through 10.255.255.255
            //  20-bit block: 172.16.0.0 through 172.31.255.255
            //  16-bit block: 192.168.0.0 through 192.168.255.255
            //  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)

            var octets = ip.GetAddressBytes();

            var is24BitBlock = octets[0] == 10;
            if (is24BitBlock) return true; // Return to prevent further processing

            var is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
            if (is20BitBlock) return true; // Return to prevent further processing

            var is16BitBlock = octets[0] == 192 && octets[1] == 168;
            if (is16BitBlock) return true; // Return to prevent further processing

            var isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;
            return isLinkLocalAddress;
        }
    }
}