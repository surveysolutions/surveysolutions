using System;

namespace WB.Core.BoundedContexts.Headquarters.Exceptions
{
    public class SurveyException : Exception
    {
        internal SurveyException(string message)
            : base(message) { }

        internal SurveyException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}