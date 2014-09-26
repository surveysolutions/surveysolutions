using System;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public class InterviewException : DataCollectionException
    {
        internal InterviewException(string message)
            : base(message) {}

        internal InterviewException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}
