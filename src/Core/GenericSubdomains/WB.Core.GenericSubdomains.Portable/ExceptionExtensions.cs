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
            if (source is TException exception)
            {
                return exception;
            }

            return source.InnerException?.GetSelfOrInnerAs<TException>();
        }

        public static Exception GetSelfOrInner(this Exception source, Func<Exception, bool> predicate)
        {
            if (predicate.Invoke(source))
            {
                return source;
            }

            return source.InnerException?.GetSelfOrInner(predicate);
        }

        public static IEnumerable<Exception> UnwrapAllInnerExceptions(this Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            yield return exception;

            IEnumerable<Exception> innerExceptions = exception is AggregateException aggregateException
                ? aggregateException.InnerExceptions
                : exception.InnerException != null 
                    ? exception.InnerException.ToEnumerable() 
                    : Array.Empty<Exception>();

            foreach (var unwrappedException in innerExceptions.SelectMany(UnwrapAllInnerExceptions))
            {
                yield return unwrappedException;
            }
        }
    }
}
