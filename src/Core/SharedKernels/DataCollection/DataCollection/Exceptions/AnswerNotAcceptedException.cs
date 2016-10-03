using System;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public class AnswerNotAcceptedException: InterviewException
    {
        public AnswerNotAcceptedException(string message) : base(message, InterviewDomainExceptionType.AnswerNotAccepted)
        {
        }

        public AnswerNotAcceptedException(string message, Exception innerException) : base(message, innerException, InterviewDomainExceptionType.AnswerNotAccepted)
        {
        }
    }
}