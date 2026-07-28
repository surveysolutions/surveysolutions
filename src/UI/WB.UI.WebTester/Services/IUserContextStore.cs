using System;

namespace WB.UI.WebTester.Services
{
    public interface IUserContextStore
    {
        /// <param name="ttl">
        /// How long the entry should be kept.  Use the same value as the delegated JWT's
        /// <c>ExpiresIn</c> so that the user context is evicted at the same time as the token.
        /// </param>
        void Store(Guid interviewId, RequestUserContext context, TimeSpan ttl);
        RequestUserContext? Get(Guid interviewId);
        void Remove(Guid interviewId);
    }
}
