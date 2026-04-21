using System;

namespace WB.UI.WebTester.Services
{
    public interface IUserContextStore
    {
        void Store(Guid interviewId, RequestUserContext context);
        RequestUserContext? Get(Guid interviewId);
        void Remove(Guid interviewId);
    }
}
