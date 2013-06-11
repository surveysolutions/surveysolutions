using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CAPI.Android.Syncronization.RestUtils
{
    public interface IRestUrils
    {
        void ExcecuteRestRequest(string url,string fileContent, params KeyValuePair<string, string>[] additionalParams);
        void ExcecuteRestRequest(string url, params KeyValuePair<string, string>[] additionalParams);
        T ExcecuteRestRequest<T>(string url, params KeyValuePair<string, string>[] additionalParams) where T : class;
    }
}