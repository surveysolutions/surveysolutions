using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RestSharp;

namespace CAPI.Android.Syncronization.RestUtils
{
    public interface IRestUrils
    {
        void ExcecuteRestRequest(string url, IAuthenticator authenticator, params KeyValuePair<string, string>[] additionalParams);
        T ExcecuteRestRequest<T>(string url, IAuthenticator authenticator, params KeyValuePair<string, string>[] additionalParams);
        T ExcecuteRestRequestAsync<T>(string url, CancellationToken ct, string requestBody, IAuthenticator authenticator, params KeyValuePair<string, string>[] additionalParams);
    }
}