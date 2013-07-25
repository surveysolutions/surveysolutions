using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    [Serializable]
    public class InterviewException : Exception
    {
        public InterviewException() {}

        public InterviewException(string message)
            : base(message) {}

        public InterviewException(string message, Exception inner)
            : base(message, inner) {}

        protected InterviewException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}
    }
}
