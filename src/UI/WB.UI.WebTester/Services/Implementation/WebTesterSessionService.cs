using System;
using Microsoft.AspNetCore.Http;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterSessionService : IWebTesterSessionService
    {
        // auth:{interviewId} = "1"  — the per-run authorization entry
        private static string AuthKey(Guid interviewId) => $"auth:{interviewId:N}";

        // qiid:{questionnaireId} = interviewId  — reverse lookup for questionnaire-only routes
        private static string QuestionnaireToInterviewKey(Guid questionnaireId) => $"qiid:{questionnaireId:N}";

        public void AuthorizeQuestionnaire(ISession session, Guid interviewId, Guid questionnaireId)
        {
            session.SetString(AuthKey(interviewId), "1");
            session.SetString(QuestionnaireToInterviewKey(questionnaireId), interviewId.ToString("N"));
        }

        public bool IsAuthorized(ISession session, Guid interviewId)
            => session.GetString(AuthKey(interviewId)) == "1";

        public void RevokeQuestionnaire(ISession session, Guid interviewId, Guid questionnaireId)
        {
            session.Remove(AuthKey(interviewId));
            session.Remove(QuestionnaireToInterviewKey(questionnaireId));
        }

        public Guid? GetInterviewId(ISession session, Guid questionnaireId)
        {
            var raw = session.GetString(QuestionnaireToInterviewKey(questionnaireId));
            if (raw != null && Guid.TryParse(raw, out var interviewId))
                return interviewId;
            return null;
        }
    }
}
