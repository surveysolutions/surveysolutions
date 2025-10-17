namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfSettings
    {
        public int InstructionsExcerptLength { get; set; }
        public int ExpressionExcerptLength { get; set; }
        public int OptionsExcerptCount { get; set; }
        public int MinAmountOfDigitsInCodes { get; set; }
        public int AttachmentSize { get; set; }
        public int PdfGenerationTimeoutInMilliseconds { get; set; }
        public int VariableExpressionExcerptLength { get; set; }
        public int WorkerCount { get; set; } = 10;
        public int MaxPerUser { get; set; } = 5;
        public int FinishedJobRetentionInMinutes { get; set; } = 24 * 60;
        public int CleanupIntervalInSeconds { get; set; } = 60 * 60;
    }
}
