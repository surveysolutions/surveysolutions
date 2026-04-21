using System;
using Microsoft.AspNetCore.Http;

namespace WB.UI.WebTester.Services
{
    public interface IWebTesterSessionService
    {
        /// <summary>
        /// Records that <paramref name="interviewId"/> (unique per run) is authorised in this
        /// browser session and stores a reverse mapping
        /// <c>questionnaireId → interviewId</c> so that routes which only carry
        /// <paramref name="questionnaireId"/> (e.g. ScenariosProxy) can still find the right key.
        /// </summary>
        void AuthorizeQuestionnaire(ISession session, Guid interviewId, Guid questionnaireId);

        /// <summary>Returns <c>true</c> when the browser session holds an active authorisation for <paramref name="interviewId"/>.</summary>
        bool IsAuthorized(ISession session, Guid interviewId);

        /// <summary>Removes the session entries for <paramref name="interviewId"/> (and its reverse mapping).</summary>
        void RevokeQuestionnaire(ISession session, Guid interviewId, Guid questionnaireId);

        /// <summary>
        /// Returns the <c>interviewId</c> stored for <paramref name="questionnaireId"/> in this
        /// browser session, or <c>null</c> when no authorised run exists for that questionnaire.
        /// </summary>
        Guid? GetInterviewId(ISession session, Guid questionnaireId);
    }
}
