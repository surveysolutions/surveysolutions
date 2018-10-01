using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
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

        public static Exception GetSelfOrInner(this Exception source, Func<Exception, bool> predicate)
        {
            if (predicate.Invoke(source))
            {
                return source;
            }

            while (source.InnerException != null)
            {
                return source.InnerException.GetSelfOrInner(predicate);
            }

            return null;
        }

        public static IEnumerable<Exception> UnwrapAllInnerExceptions(this Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");

            yield return exception;

            IEnumerable<Exception> innerExceptions = exception is AggregateException
                ? ((AggregateException) exception).InnerExceptions
                : exception.InnerException != null ? exception.InnerException.ToEnumerable() : new Exception[] { };

            foreach (var unwrappedException in innerExceptions.SelectMany(UnwrapAllInnerExceptions))
            {
                yield return unwrappedException;
            }
        }
    }
}
