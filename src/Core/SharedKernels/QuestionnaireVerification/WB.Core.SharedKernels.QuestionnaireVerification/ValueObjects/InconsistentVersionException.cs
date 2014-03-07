using System;
using System.Runtime.Serialization;

namespace WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects
{
    [DataContract]
    public class InconsistentVersionException : Exception
    {
        [DataMember]
        public string Reason { get; set; }
    }
}
