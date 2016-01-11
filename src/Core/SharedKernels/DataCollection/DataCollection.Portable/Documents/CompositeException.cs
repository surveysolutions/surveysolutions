using System;

namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class CompositeException : Exception
    {
        public CompositeException()
        {
        }
        
        public CompositeException(string message)
            : base(message)
        {
        }

        public CompositeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}