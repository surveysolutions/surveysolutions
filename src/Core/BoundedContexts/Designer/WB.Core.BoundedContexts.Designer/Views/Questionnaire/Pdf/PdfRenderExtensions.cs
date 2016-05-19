using WB.Core.BoundedContexts.Designer.Commands;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public static class PdfRenderExtensions
    {
        public static string SanitizeHtml(this string text) => CommandUtils.SanitizeHtml(text, true);
    }
}