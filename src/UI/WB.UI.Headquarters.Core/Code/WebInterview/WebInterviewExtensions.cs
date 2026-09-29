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

        public static void SetPasswordVerifiedForInterview(this ISession session, string interviewId)
        {
            var interviews = session.Get<List<string>>(PasswordVerifiedKey) ?? new List<string>();
            if (!interviews.Contains(interviewId))
            {
                interviews.Add(interviewId);
            }
            session.Set(PasswordVerifiedKey, interviews);
        }
    }
}
