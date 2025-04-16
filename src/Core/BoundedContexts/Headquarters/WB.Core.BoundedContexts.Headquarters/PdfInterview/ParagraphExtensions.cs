#nullable enable

using System.Net;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview
{
    public static class ParagraphExtensions
    {
        public static FormattedText AddParagraphFormattedText(this Cell cell, string text, string styleName)
        {
            var paragraph = cell.AddParagraph();
            paragraph.Style = styleName;
            text = WebUtility.HtmlDecode(text);
            var formattedText = paragraph.AddFormattedText(text);
            formattedText.Font.Name = PdfInterviewFontResolver.GetFontNameForText(text);
            return formattedText;
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
            formattedText.Font.Name = PdfInterviewFontResolver.GetFontNameForText(text);
            return formattedText;
        }
        
        public static void AddWrappedText(this Paragraph paragraph, string text)
        {
            text = WebUtility.HtmlDecode(text);
            for (int i = 0; i < text.Length; i+=15)
            {
                var str = i + 15 > text.Length
                    ? text.Substring(i)
                    : text.Substring(i, 15);
                
                paragraph.Format.Font.Name = PdfInterviewFontResolver.GetFontNameForText(str);
                paragraph.AddText(str);
            }
        }

        public static void AddWrapFormattedText(this Paragraph paragraph, string text, string style, Color? color = null)
        {
            if(string.IsNullOrEmpty(text))
                return;
            
            text = WebUtility.HtmlDecode(text);
            for (int i = 0; i < text.Length; i+=15)
            {
                var str = i + 15 > text.Length
                    ? text.Substring(i)
                    : text.Substring(i, 15);
                
                var formattedText = paragraph.AddFormattedText(str, style);
                if (color.HasValue)
                    formattedText.Color = color.Value;
                formattedText.Font.Name = PdfInterviewFontResolver.GetFontNameForText(str);
            }
        }
    }
}
