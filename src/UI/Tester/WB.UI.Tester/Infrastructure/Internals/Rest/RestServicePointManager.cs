using System.Net;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Tester.Infrastructure.Internals.Rest
{
    public class RestServicePointManager : IRestServicePointManager
    {
        public void AcceptUnsignedSslCertificate()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }
    }
}