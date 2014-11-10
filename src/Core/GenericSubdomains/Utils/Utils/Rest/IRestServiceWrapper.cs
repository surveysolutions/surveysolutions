using System.Collections.Generic;
using System.Threading;

namespace WB.Core.GenericSubdomains.Utils.Rest
{
    public interface IRestServiceWrapper
    {
        void ExecuteRestRequest(string url, string login, string password, string method, params KeyValuePair<string, object>[] additionalParams);
        T ExecuteRestRequest<T>(string url, string login, string password, string method, params KeyValuePair<string, object>[] additionalParams);
        T ExecuteRestRequestAsync<T>(string url, CancellationToken ct, object requestBody, string login, string password, string method, params KeyValuePair<string, object>[] additionalParams);

        T ExecuteRestRequestAsync<T>(string url, KeyValuePair<string, object>[] queryStringParams, CancellationToken ct, byte[] file, string fileName, string login, string password, string method, params KeyValuePair<string, object>[] additionalParams);
        void ExecuteRestRequestAsync(string url, CancellationToken ct, byte[] file, string fileName, string login, string password, string method, params KeyValuePair<string, object>[] additionalParams);
    }
}