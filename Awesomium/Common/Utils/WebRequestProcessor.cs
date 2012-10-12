using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Awesomium.Core;

namespace Common.Utils
{
    public class WebRequestProcessor : IRequesProcessor
    {
        #region Implementation of IRequesProcessor

        public T Process<T>(string url, T defaultValue)// where T : struct
        {
            return Process<T>(url, "GET", defaultValue);
        }

        public T Process<T>(string url, string method, T defaultValue)// where T : struct
        {
            return Process<T>(url, method, false, defaultValue);
        }

        public T Process<T>(string url, string method, bool includeCookies, T defaultValue)// where T : struct
        {
            try
            {
                var uri = new Uri(url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = method;
                if (includeCookies)
                {
                    var cookies = WebCore.GetCookies(url, false);
                    request.CookieContainer = new CookieContainer();
                    foreach (string cookie in cookies.Split(';'))
                    {
                        string name = cookie.Split('=')[0];
                        string value = cookie.Substring(name.Length + 1);
                        string path = "/";
                        string domain = uri.Host; //change to your domain name
                        request.CookieContainer.Add(new Cookie(name.Trim(), value.Trim(), path, domain));
                    }
                }
                // Get the response.
                using (WebResponse response = request.GetResponse())
                {
                    // Get the stream containing content returned by the server.
                    var dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();

                    try
                    {
                        return
                            (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(responseFromServer);
                    }
                    finally
                    {
                        // Clean up the streams.
                        reader.Close();
                        dataStream.Close();
                        response.Close();
                    }
                }
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        #endregion
    }
}
