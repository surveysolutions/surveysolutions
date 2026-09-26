using System.Linq;
using System.Net.Http.Headers;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;

namespace WB.Core.Infrastructure.HttpServices.Services;

public class IntegrityService : IIntegrityService
{
    public const string IntegrityHeaderValue = "773994826649214";
    public const string IntegrityHeaderName = "X-Survey-Solutions";

    private readonly IRestServiceSettings enumeratorSettings;

    public IntegrityService(IRestServiceSettings enumeratorSettings)
    {
        this.enumeratorSettings = enumeratorSettings;
    }

    public void ValidateResponseHeadersAndThrow(HttpResponseHeaders headers)
    {
        if (this.enumeratorSettings.CommunicationIntegrityValidationIgnore)
            return;
        
        if (!headers.Contains(IntegrityHeaderName) 
            || headers.GetValues(IntegrityHeaderName).All(v => v != IntegrityHeaderValue))
        {
            throw new RestException("Response integrity validation failed",
                type: RestExceptionType.CommunicationIntegrityValidationFailed); 
        }
    }
}
