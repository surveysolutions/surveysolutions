using System.Runtime.Serialization;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
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