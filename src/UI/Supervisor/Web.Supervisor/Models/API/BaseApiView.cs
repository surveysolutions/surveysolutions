using System.Runtime.Serialization;

namespace Web.Supervisor.Models.API
{
    public abstract class BaseApiView
    {
        [DataMember]
        public string Order { get; protected set; }

        [DataMember]
        public int Limit { get; protected set; }

        [DataMember]
        public int TotalCount { get; protected set; }

        [DataMember]
        public int Offset { get; protected set; }
    }
}