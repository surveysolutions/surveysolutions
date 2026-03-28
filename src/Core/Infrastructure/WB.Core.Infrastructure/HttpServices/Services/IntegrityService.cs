using System.Linq;
using System.Net.Http.Headers;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;

namespace WB.Core.Infrastructure.HttpServices.Services;

public class IntegrityService : IIntegrityService
{
    private readonly string HeaderValue = "773994826649214";
    private string XSurveySolutions = "X-Survey-Solutions";
    
    private readonly IRestServiceSettings enumeratorSettings;

    public IntegrityService(IRestServiceSettings enumeratorSettings)
    {
        this.enumeratorSettings = enumeratorSettings;
    }

    public void ValidateResponseHeadersAndThrow(HttpResponseHeaders headers)
    {
        if(enumeratorSettings.CommunicationIntegrityValidationIgnore)
            return;
        
        if (!headers.Contains(XSurveySolutions) 
                || headers.GetValues(XSurveySolutions).All(v => v != HeaderValue))
            {                
                throw new RestException("The response received does not appear to come from the Survey Solutions server. This is usually caused by a network proxy, firewall, or security gateway intercepting the connection. Please check your network connection or contact your IT support.");
            }
    }
}
