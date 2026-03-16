using System.Net.Http.Headers;

namespace WB.Core.Infrastructure.HttpServices.Services;

public interface IIntegrityService
{
    void ValidateResponseHeadersAndThrow(HttpResponseHeaders content);
}
