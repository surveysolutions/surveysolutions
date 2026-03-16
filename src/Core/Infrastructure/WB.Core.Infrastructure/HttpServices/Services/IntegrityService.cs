using System.Linq;
using System.Net.Http.Headers;
using WB.Core.Infrastructure.HttpServices.HttpClient;

namespace WB.Core.Infrastructure.HttpServices.Services;

public class IntegrityService : IIntegrityService
{
    private readonly string HeaderValue = "773994826649214";
    private string XSurveySolutions = "X-Survey-Solutions";

    public void ValidateResponseHeadersAndThrow(HttpResponseHeaders headers)
    {
            if (!headers.Contains(XSurveySolutions) 
                || headers.GetValues(XSurveySolutions).All(v => v != HeaderValue)))
            {                
                throw new RestException("Communication error: error response from server. Please contact your administrator.");
            }
    }
}
