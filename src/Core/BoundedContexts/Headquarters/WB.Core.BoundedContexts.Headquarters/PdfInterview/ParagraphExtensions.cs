#nullable enable

using System.Collections.Generic;
using System.Net;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview
{
    public static class ParagraphExtensions
    {
        public static FormattedText AddParagraphFormattedText(this Cell cell, string text, string styleName)
        {
            var paragraph = cell.AddParagraph();
            paragraph.Style = styleName;
            text = WebUtility.HtmlDecode(text);
            return paragraph.AddFormattedText(text);
        }
        
        public static FormattedText AddFormattedText(this Paragraph paragraph, string text, bool? isBold = null, bool? isItalic = null, Unit? size = null)
        {
            text = WebUtility.HtmlDecode(text);
            var formattedText = paragraph.AddFormattedText(text);
            if (isBold.HasValue)
                formattedText.Font.Bold = isBold.Value;
            if (isItalic.HasValue)
                formattedText.Font.Italic = isItalic.Value;
            if (size.HasValue)
                formattedText.Font.Size = size.Value;
            return formattedText;
        }
        
        public static void AddWrappedText(this Paragraph paragraph, string text)
        {
            text = WebUtility.HtmlDecode(text);
            foreach (var str in SplitToWrap(text))
                paragraph.AddText(str);
        }

        public static void AddWrapFormattedText(this Paragraph paragraph, string text, string style, Color? color = null)
        {
            if(string.IsNullOrEmpty(text))
                return;
            
            text = WebUtility.HtmlDecode(text);
            foreach (var str in SplitToWrap(text))
            {
                var formattedText = paragraph.AddFormattedText(str, style);
                if (color.HasValue)
                    formattedText.Color = color.Value;
            }
        }

        /// <summary>
        /// Long words without spaces can be wrapped only between chunks. Right-to-left text is not split:
        /// PDFsharp orders every chunk separately, so words of split right-to-left text get mixed up.
        /// </summary>
        private static IEnumerable<string> SplitToWrap(string text)
        {
            if (PdfRtlText.ContainsRightToLeft(text))
            {
                yield return text;
                yield break;
            }

            for (int i = 0; i < text.Length; i+=15)
            {
                yield return i + 15 > text.Length
                    ? text.Substring(i)
                    : text.Substring(i, 15);
            }
        }
    }
}