using System;
using System.Globalization;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public static class EnumeratorSettingsHelpers
    {
        public static string RenderWebInterviewUri(this IEnumeratorSettings setting, int assignmentId,
            Guid interviewId)
        {
            var template = setting.WebInterviewUriTemplate;
            template = string.IsNullOrEmpty(template) ? setting.Endpoint + "/webinterview/link/{assignment}/{interviewId}" : template;

            return template
                .Replace("{assignment}", assignmentId.ToString(CultureInfo.InvariantCulture))
                .Replace("{interviewId}", interviewId.ToString());
        }
    }
}
