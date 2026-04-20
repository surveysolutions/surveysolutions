using System;
using Microsoft.AspNetCore.Http;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterSessionService : IWebTesterSessionService
    {
        private static string Key(Guid id) => $"qauth:{id:N}";

        public void AuthorizeQuestionnaire(ISession session, Guid questionnaireId)
            => session.SetString(Key(questionnaireId), "1");

        public bool IsAuthorized(ISession session, Guid questionnaireId)
            => session.GetString(Key(questionnaireId)) == "1";

        public void RevokeQuestionnaire(ISession session, Guid questionnaireId)
            => session.Remove(Key(questionnaireId));
    }
}
