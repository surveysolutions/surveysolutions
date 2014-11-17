using System;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    [Serializable]
    public class InterviewException : Exception
    {
        internal InterviewException(string message)
            : base(message) {}

        internal InterviewException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}
