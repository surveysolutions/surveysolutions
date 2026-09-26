using System;

namespace WB.UI.WebTester.Services
{
    public interface IWebTesterJwtStore
    {
        void StoreToken(Guid interviewId, string jwt, TimeSpan expiresIn);
        string? GetToken(Guid interviewId);
        void Remove(Guid interviewId);
    }
}
