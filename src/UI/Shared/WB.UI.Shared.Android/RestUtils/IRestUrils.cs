using System.Collections.Generic;
using System.Threading;
using RestSharp;

namespace WB.UI.Shared.Android.RestUtils
{
    public interface IRestUrils
    {
        void ExcecuteRestRequest(string url, IAuthenticator authenticator, string method, params KeyValuePair<string, string>[] additionalParams);
        T ExcecuteRestRequest<T>(string url, IAuthenticator authenticator, string method, params KeyValuePair<string, string>[] additionalParams);
        T ExcecuteRestRequestAsync<T>(string url, CancellationToken ct, object requestBody, IAuthenticator authenticator, string method, params KeyValuePair<string, string>[] additionalParams);
    }
}