using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.BarCodes;
using PdfSharpCore.Pdf;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using ZXing;
using ZXing.OneD;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class WebInterviewEmailTemplate
    {
        public string Subject { get; }
        public string MainText { get; }
        public string PasswordDescription { get; }
        public string LinkText { get; }

        public WebInterviewEmailTemplate(string subject, string mainText, string passwordDescription, string linkText)
        {
            this.Subject = subject;
            this.MainText = mainText;
            this.PasswordDescription = passwordDescription;
            this.LinkText = linkText;
        }
    }

    public class EmailContent
    {
        private const string Password = "%PASSWORD%";
        private const string SurveyLink = "%SURVEYLINK%";
        private const string SurveyName = "%SURVEYNAME%";
        private const string QuestionnaireTitle = "%QUESTIONNAIRE%";

        public EmailContent(WebInterviewEmailTemplate template, string questionnaireTitle, string link, string password)
        {
            Subject = template.Subject
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
            MainText = template.MainText
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle)
                .Replace(SurveyLink, link)
                .Replace(Password, password);
            LinkText = template.LinkText?
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
            PasswordDescription = template.PasswordDescription?
                .Replace(SurveyName, questionnaireTitle)
                .Replace(QuestionnaireTitle, questionnaireTitle);
        }

        public string Subject { get; private set; }
        public string MainText { get; private set; }
        public string PasswordDescription { get; }
        public string LinkText { get; }
        
        public List<EmailAttachment> Attachments { get; } = new List<EmailAttachment>();

        public void RenderInterviewData(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            Subject = ReplaceVariablesWithData(Subject, interview, questionnaire);
            MainText = ReplaceVariablesWithData(MainText, interview, questionnaire);
        }

        private static Regex FindBarcodeVariables = new Regex("%[A-Za-z0-9_]+:barcode%", RegexOptions.Compiled);
        private static Regex FindVariables = new Regex("%[A-Za-z0-9_]+%", RegexOptions.Compiled);
        private string ReplaceVariablesWithData(string text, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            text = FindBarcodeVariables.Replace(text, match =>
            {
                var variable = match.Value.Trim('%').Replace(":barcode", "");
                var questionId = questionnaire.GetQuestionIdByVariable(variable);
                if (!questionId.HasValue)
                    return String.Empty;
                var answer = interview.GetAnswerAsString(new Identity(questionId.Value, RosterVector.Empty));

                var attachment = CreateBarCodeAttachment(answer);
                Attachments.Add(attachment);
                
                return $"<img alt='barcode' src='cid:{attachment.ContentId}'/>";
            });
            
            return FindVariables.Replace(text, match =>
            {
                var questionId = questionnaire.GetQuestionIdByVariable(match.Value.Trim('%'));
                if (!questionId.HasValue)
                    return String.Empty;
                var answer = interview.GetAnswerAsString(new Identity(questionId.Value, RosterVector.Empty));
                return answer;
            });
        }

        private EmailAttachment CreateBarCodeAttachment(string answer)
        {
            var barCodeStream = RenderBarCode(answer);
            var id = Guid.NewGuid();
            
            return new EmailAttachment()
            {
                Base64String = Convert.ToBase64String(barCodeStream.ToArray()),
                ContentType = "image/jpeg",
                Filename = id.ToString() + ".jpeg",
                ContentId = id.ToString(),
                Disposition = EmailAttachmentDisposition.Inline,
            };
        }

        private MemoryStream RenderBarCode(string text)
        {
            var width = 149;
            var height = 53;
            // Code128Writer writer = new Code128Writer();
            // var bm = writer.encode(text, BarcodeFormat.CODE_128, width, 1);

            MultiFormatWriter writer = new MultiFormatWriter();

            // Use 1 as the height of the matrix as this is a 1D Barcode.
            var bm = writer.encode(text, BarcodeFormat.CODE_128, width, 1);
            int bmWidth = bm.Width;

            Bitmap imageBitmap = new Bitmap(bmWidth, height, PixelFormat.Format32bppArgb);
            // Bitmap imageBitmap = Bitmap.createBitmap(bmWidth, height, Config.ARGB_8888);

            for (int x = 0; x < bmWidth; x++) 
            {
                var color = bm[x, 0] ? Color.Black : Color.White;
                for (int y = 0; y < height; y++)
                {
                    imageBitmap.SetPixel(x, y, color);
                }
            }
            
            //imageBitmap.Save("c:\\Temp\\barcode.jpeg", ImageFormat.Jpeg);

            var ms = new MemoryStream();
            imageBitmap.Save(ms, ImageFormat.Jpeg);
            return ms;
            
            // MultiFormatWriter writer = new MultiFormatWriter();
            // writer.encode()
            // BarcodeWriter<> barcodeWriter = new BarcodeWriter<>();
            // BarcodeWriterGeneric generic = new 
                

            /*text = text.ToUpper();
            // XImage xImage = XBitmapImage.CreateBitmap(149, 53);
            XGraphics gfx = XGraphics.FromImage(xImage);
            XGraphics gfx = XGraphics.CreateMeasureContext(new XSize(149, 53), 
                XGraphicsUnit.Point, XPageDirection.Downwards);
            
            // PdfDocument pdfd = new PdfDocument();
            // XGraphics gfx = XGraphics.FromPdfPage(pdfd.AddPage());
            
            BarCode barcode = BarCode.FromType(CodeType.Code3of9Standard, text);
            barcode.TextLocation = new TextLocation();
            barcode.Text = text;
            barcode.StartChar = Convert.ToChar("*");
            barcode.EndChar = Convert.ToChar("*");
            barcode.Direction = CodeDirection.LeftToRight;
            XFont fontBarcode = new XFont("Arial", 14, XFontStyle.Regular);
            var position = new XPoint(Convert.ToDouble(0), Convert.ToDouble(0));
            XSize size = new XSize(Convert.ToDouble(149), Convert.ToDouble(53));
            barcode.Size = size;
            gfx.DrawBarCode(barcode, XBrushes.Black, fontBarcode, position);
            Bitmap b = new Bitmap(143, 53, gfx.);
            //gfx.
            return xImage.AsJpeg();*/
        }

    }
}


