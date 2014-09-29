using System;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public abstract class DataCollectionException : Exception
    {
        internal DataCollectionException(string message)
            : base(message) {}

        internal DataCollectionException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}
