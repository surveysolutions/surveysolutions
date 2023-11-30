using System;
using System.Runtime.Serialization;

namespace WB.Core.BoundedContexts.Headquarters.Services.Internal
{
    [Serializable]
    public class InterviewUniqueKeyGeneratorException : Exception
    {
        public InterviewUniqueKeyGeneratorException()
        {
        }

        public InterviewUniqueKeyGeneratorException(string message) : base(message)
        {
        }

        public InterviewUniqueKeyGeneratorException(string message, Exception inner) : base(message, inner)
        {
        }        
    }
}
