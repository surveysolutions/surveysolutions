using System;

namespace WB.UI.Shared.Web.Extensions
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
    }
}