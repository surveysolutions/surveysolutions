using System;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public class QuestionnaireException : Exception
    {
        public QuestionnaireException(string message)
            : base(message) {}

        public QuestionnaireException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}