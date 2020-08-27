using System;

namespace WB.Core.Infrastructure.HttpServices.HttpClient
{
    public class ExtendedMessageHandlerException : Exception
    {
        /// <summary>
        /// An object containing details about the failed HTTP call
        /// </summary>
        public HttpCall Call { get; }

        public ExtendedMessageHandlerException(HttpCall call, string message, Exception inner) : base(message, inner) {
            Call = call;
        }

        public ExtendedMessageHandlerException(HttpCall call, Exception inner) : this(call, BuildMessage(call, inner), inner) { }

        public ExtendedMessageHandlerException(HttpCall call) : this(call, BuildMessage(call, null), null) { }

        private static string BuildMessage(HttpCall call, Exception inner)
        {
            if (call.Response != null && !call.IsSucceeded)
            {
                return string.Format("Request {0} {1} failed with status code {2} ({3}).",
                    call.Request.Method,
                    call.Request.RequestUri.AbsoluteUri,
                    (int)call.Response.StatusCode,
                    call.Response.ReasonPhrase);
            }
            if (inner != null)
            {
                return $"Request to {call.Request.RequestUri.AbsoluteUri} failed. {inner.Message}";
            }

            return $"Request to {call.Request.RequestUri.AbsoluteUri} failed.";
        }
    }
}
