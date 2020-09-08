using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.UI.Headquarters.Code.WebInterview
{
    public static class WebInterviewExtensions
    {
        public static void SaveWebInterviewAccessForCurrentUser(this HttpContext context, string interviewId)
        {
            context.Session.Set("WebInterview-" + interviewId, true);
        }
        
        public static bool HasAccessToWebInterviewAfterComplete(this HttpContext context, IStatefulInterview interview)
        {
            var isExistsInterviewInCookie = context.Session.Get<bool>("WebInterview-" + interview.Id.FormatGuid());
            var hasAccess = isExistsInterviewInCookie && interview.CompletedDate.HasValue
                                                      && interview.CompletedDate.Value.UtcDateTime.AddHours(1) > DateTime.UtcNow;
            return hasAccess;
        }
    }
}