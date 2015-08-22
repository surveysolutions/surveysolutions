using System;
using System.Runtime.Serialization;

namespace WB.UI.Interviewer.Syncronization
{
    public class SynchronizationException : Exception
    {
        public SynchronizationException()
        {
        }

        public SynchronizationException(string message) : base(message)
        {
        }

        protected SynchronizationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SynchronizationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}