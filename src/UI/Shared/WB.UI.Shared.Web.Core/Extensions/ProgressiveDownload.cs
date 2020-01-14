using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace WB.UI.Shared.Web.Extensions
{
    public class ProgressiveDownload
    {
        private readonly HttpRequest request;

        public ProgressiveDownload(HttpRequest request)
        {
            this.request = request;
        }

        public HttpResponseMessage ResultMessage(Stream stream, string mediaType, string fileName = null, byte[] hash = null)
        {
            return new HttpResponseMessage()
            {
                Content = new StreamContent(stream),
            };
        }
    }
}
