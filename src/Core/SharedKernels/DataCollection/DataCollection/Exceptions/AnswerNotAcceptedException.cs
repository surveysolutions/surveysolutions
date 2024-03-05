using System;
using System.Runtime.Serialization;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    [Serializable]
    public class AnswerNotAcceptedException: InterviewException
    {
        public AnswerNotAcceptedException(string message) : base(message, InterviewDomainExceptionType.AnswerNotAccepted)
        {
        }

        public AnswerNotAcceptedException(string message, Exception innerException) : base(message, innerException, InterviewDomainExceptionType.AnswerNotAccepted)
        {
        }

        protected AnswerNotAcceptedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ExceptionType = (InterviewDomainExceptionType)info.GetInt32(nameof(ExceptionType));
        }
    }
}