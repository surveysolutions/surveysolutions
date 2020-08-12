namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfSettings
    {
        public string WKHtmlToPdfExecutablePath { get; set; } = "";

        public int InstructionsExcerptLength { get; set; }
        public int ExpressionExcerptLength { get; set; }
        public int OptionsExcerptCount { get; set; }
        public int MinAmountOfDigitsInCodes { get; set; }
        public int AttachmentSize { get; set; }
        public int PdfGenerationTimeoutInMilliseconds { get; set; }
        public int VariableExpressionExcerptLength { get; set; }
        public string WkHtmlToPdfExeName { get; set; } = "wkhtmltopdf.exe";
    }
}
