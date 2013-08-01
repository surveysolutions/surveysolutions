using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public class InterviewException : Exception
    {
        internal InterviewException(string message)
            : base(message) {}

        internal InterviewException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}
