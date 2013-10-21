using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Core.Domain.Exceptions
{
    public class CompleteQuestionnaireException : Exception
    {
        internal CompleteQuestionnaireException(string message)
            : base(message) { }

        internal CompleteQuestionnaireException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
