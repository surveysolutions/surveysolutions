using System;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public class QuestionnaireException : DataCollectionException
    {
        internal QuestionnaireException(string message)
            : base(message) {}

        internal QuestionnaireException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}