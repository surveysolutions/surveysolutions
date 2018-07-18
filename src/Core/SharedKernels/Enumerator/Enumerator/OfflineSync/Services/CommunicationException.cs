using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    [ExcludeFromCodeCoverage]
    public class CommunicationException : Exception
    {
        static string ToMessage(string endpoint, object message, Type responseType)
        {
            return $"Error while sending <{message.GetType().Name}, {responseType.Name}> to endpoint '{endpoint}'";
        }

        public CommunicationException(
            Exception innerException, 
            INearbyConnection connection,
            string endpoint, 
            object message, Type responseType) 
            : base(ToMessage(endpoint, message, responseType), innerException)
        {
            if(innerException.Data != null)
            foreach (var key in innerException.Data.Keys)
            {
                this.Data[key] = innerException.Data[key];
            }

            if (message != null)
            {
                this.Data["requestType"] = message.GetType().Name;
                this.Data["message"] = JsonConvert.SerializeObject(message, Formatting.None);
            }

            this.Data["responseType"] = responseType.Name;
            this.Data["endpoint"] = endpoint;

            var remote = connection.RemoteEndpoints.SingleOrDefault(r => r.Enpoint == endpoint);
            if (remote != null)
            {
                this.Data["remote"] = remote.Name;
            }
        }
    }
}
