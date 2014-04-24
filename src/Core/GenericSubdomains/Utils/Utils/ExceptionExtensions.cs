using System;
using System.Collections.Generic;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class ExceptionExtensions
    {
        public static TException GetSelfOrInnerAs<TException>(this Exception source)
            where TException : Exception
        {
            if (source is TException)
            {
                return (TException) source;
            }

            while (source.InnerException != null)
            {
                return source.InnerException.GetSelfOrInnerAs<TException>();
            }

            return null;
        }

        public static IEnumerable<Exception> UnwrapAllInnerExceptions(this Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            yield return exception;

            Exception innerException = exception.InnerException;
            
            while (innerException != null)
            {
                yield return innerException;
                innerException = innerException.InnerException;
            }
        }
    }
}