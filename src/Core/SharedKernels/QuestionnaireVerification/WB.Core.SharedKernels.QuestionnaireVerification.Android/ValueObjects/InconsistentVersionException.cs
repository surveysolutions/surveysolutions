using System.Runtime.Serialization;

namespace WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects
{
    [DataContract]
    public class InconsistentVersionException
    {
        [DataMember]
        public string Reason { get; set; }
    }
}
