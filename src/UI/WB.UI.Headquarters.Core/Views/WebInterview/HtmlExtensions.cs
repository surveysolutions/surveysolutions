using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WB.UI.Headquarters.Views.WebInterview
{
    public static class HtmlExtensions
    {
        public static IHtmlContent SubstituteQuestionnaireName(this IHtmlHelper html,
            string template,
            string questionnaireName)
        {
            if (string.IsNullOrWhiteSpace(template)) return HtmlString.Empty;

            return new HtmlString(template.Replace("%QUESTIONNAIRE%", questionnaireName).Replace("%SURVEYNAME%", questionnaireName));
        }

    }
}
