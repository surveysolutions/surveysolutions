using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Core.Domain.Exceptions
{
    public class InterviewException : Exception
    {
        internal InterviewException(string message)
            : base(message) { }

        internal InterviewException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
