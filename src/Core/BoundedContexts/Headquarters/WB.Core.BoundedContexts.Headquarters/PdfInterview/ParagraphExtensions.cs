#nullable enable

using System.Net;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview
{
    public static class ParagraphExtensions
    {
        public static void AddParagraphFormattedText(this Cell cell, string text, string styleName)
        {
            var paragraph = cell.AddParagraph();
            paragraph.Style = styleName;
            text = WebUtility.HtmlDecode(text);

            foreach (var part in FontScriptDetector.SplitByScript(text))
            {
                var formatted = paragraph.AddFormattedText(part.Fragment);
                formatted.Font.Name = part.Font;
            }
        }

        public static void AddFormattedText(this Paragraph paragraph, string text, bool? isBold = null, bool? isItalic = null, Unit? size = null)
        {
            text = WebUtility.HtmlDecode(text);

            foreach (var part in FontScriptDetector.SplitByScript(text))
            {
                var formattedText = paragraph.AddFormattedText(part.Fragment);

                formattedText.Font.Name = part.Font;

                if (isBold.HasValue)
                    formattedText.Font.Bold = isBold.Value;
                if (isItalic.HasValue)
                    formattedText.Font.Italic = isItalic.Value;
                if (size.HasValue)
                    formattedText.Font.Size = size.Value;
            }
        }

        public static void AddWrappedText(this Paragraph paragraph, string text)
        {
            text = WebUtility.HtmlDecode(text);
            for (int i = 0; i < text.Length; i += 15)
            {
                var str = i + 15 > text.Length
                    ? text.Substring(i)
                    : text.Substring(i, 15);

                foreach (var part in FontScriptDetector.SplitByScript(str))
                {
                    paragraph.Format.Font.Name = part.Font;
                    paragraph.AddText(part.Fragment);
                }
            }
        }

        public static void AddWrapFormattedText(this Paragraph paragraph, string text, string style, Color? color = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            text = WebUtility.HtmlDecode(text);
            for (int i = 0; i < text.Length; i += 15)
            {
                var str = i + 15 > text.Length
                    ? text.Substring(i)
                    : text.Substring(i, 15);

                foreach (var part in FontScriptDetector.SplitByScript(str))
                {
                    var formattedText = paragraph.AddFormattedText(part.Fragment, style);
                    if (color.HasValue)
                        formattedText.Color = color.Value;

                    formattedText.Font.Name = part.Font;
                }
            }
        }
    }
}
