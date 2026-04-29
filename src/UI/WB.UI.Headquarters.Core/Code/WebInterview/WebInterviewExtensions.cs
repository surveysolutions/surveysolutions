using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.UI.Headquarters.Code.WebInterview
{
    public static class WebInterviewExtensions
    {
        public const string PasswordVerifiedKey = "PasswordVerifiedKey";

        public static void SaveWebInterviewAccessForCurrentUser(this ISession session, string interviewId)
        {
            session.Set("WebInterview-" + interviewId, true);
        }
        
        public static bool HasAccessToWebInterviewAfterComplete(this ISession session, IStatefulInterview interview)
        {
            return session.Get<bool>("WebInterview-" + interview.Id.FormatGuid());
        }

        public static bool IsPasswordVerifiedForInterview(this ISession session, string interviewId)
        {
            var passedInterviews = session.Get<List<string>>(PasswordVerifiedKey);
            return passedInterviews?.Contains(interviewId) ?? false;
        }
    }
}
