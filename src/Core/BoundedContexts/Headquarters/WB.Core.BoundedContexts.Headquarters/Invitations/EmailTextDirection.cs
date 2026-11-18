#nullable enable

using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    /// <summary>
    /// Direction of an email follows the first letter of its authored text, as `dir="auto"` does in browsers.
    /// Email clients (Outlook for Windows in particular) ignore `dir="auto"`, so the direction is resolved on the server.
    /// </summary>
    public static class EmailTextDirection
    {
        private const char RightToLeftMark = '\u200F';

        // substituted values (questionnaire title, link, answers) do not define the language of the email
        private static readonly Regex Placeholders =
            new Regex("%[A-Za-z0-9_]+(:[a-z]+)?%", RegexOptions.Compiled, System.TimeSpan.FromMilliseconds(1000));

        private static readonly Regex HtmlTags =
            new Regex("<[^>]*>", RegexOptions.Compiled, System.TimeSpan.FromMilliseconds(1000));

        public static bool IsRightToLeft(WebInterviewEmailTemplate template)
        {
            var texts = new[] { template.Subject, template.MainText, template.LinkText, template.PasswordDescription }
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(text => WebUtility.HtmlDecode(HtmlTags.Replace(Placeholders.Replace(text!, " "), " ")));

            return PdfRtlText.StartsRightToLeft(string.Join("\n", texts));
        }

        /// <summary>Plain text has no markup, a right-to-left mark makes every line a right-to-left paragraph.</summary>
        public static string MarkRightToLeftLines(string text) =>
            string.Join("\n", text.Split('\n')
                .Select(line => string.IsNullOrWhiteSpace(line) ? line : RightToLeftMark + line));
    }
}
