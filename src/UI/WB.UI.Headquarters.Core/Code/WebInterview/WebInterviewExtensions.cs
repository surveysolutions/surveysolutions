using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.UI.Headquarters.Code.WebInterview
{
    public static class WebInterviewExtensions
    {
        public static bool HasInterviewInfoInCookie(this IRequestCookieCollection cookieCollection, Guid interviewId)
        {
            return cookieCollection.Keys.Where(key => key.StartsWith($"InterviewId-"))
                .Any(key =>
                    Guid.TryParse(cookieCollection[key], out Guid cookieInterviewId)
                    && cookieInterviewId == interviewId
                );
        }
        
        public static bool HasAccessToWebInterviewAfterComplete(this HttpRequest request, IStatefulInterview interview)
        {
            var isExistsInterviewInCookie = request.Cookies.HasInterviewInfoInCookie(interview.Id);
            var hasAccess = isExistsInterviewInCookie && interview.CompletedDate.HasValue
                                                      && interview.CompletedDate.Value.UtcDateTime.AddHours(1) > DateTime.UtcNow;
            return hasAccess;
        }
    }
}