using System;
using Microsoft.AspNetCore.Http;

namespace WB.UI.WebTester.Services
{
    public interface IWebTesterSessionService
    {
        void AuthorizeQuestionnaire(ISession session, Guid questionnaireId);
        bool IsAuthorized(ISession session, Guid questionnaireId);
        void RevokeQuestionnaire(ISession session, Guid questionnaireId);
    }
}
