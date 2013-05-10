using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WB.UI.Shared.Web.Exceptions
{
    public class HttpStatusException : ApplicationException
    {
        public HttpStatusCode StatusCode { get; private set; }

        public HttpStatusException(HttpStatusCode code)
        {
            this.StatusCode = code;
        }

        public HttpStatusException(string message) : base(message)
        {
        }

        public HttpStatusException(string message, Exception inner) : base(message, inner)
        {
        }

        protected HttpStatusException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
