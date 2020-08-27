using MigraDocCore.DocumentObjectModel;

namespace WB.UI.Headquarters.PdfInterview
{
    public static class ParagraphExtensions
    {
        public static void AddWrappedText(this Paragraph paragraph, string text)
        {
            for (int i = 0; i < text.Length; i+=15)
            {
                var str = i + 15 > text.Length
                    ? text.Substring(i)
                    : text.Substring(i, 15);
                paragraph.AddText(str);
            }
        }

        public static void AddWrapFormattedText(this Paragraph paragraph, string text, string style)
        {
            for (int i = 0; i < text.Length; i+=15)
            {
                var str = i + 15 > text.Length
                    ? text.Substring(i)
                    : text.Substring(i, 15);
                paragraph.AddFormattedText(str, style);
            }
        }
    }
}