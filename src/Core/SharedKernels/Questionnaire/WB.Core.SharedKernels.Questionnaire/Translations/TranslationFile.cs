namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public class TranslationFile
    {
        public TranslationFile(string questionnaireTitle, byte[] contentAsExcelFile, string translationName)
        {
            QuestionnaireTitle = questionnaireTitle;
            ContentAsExcelFile = contentAsExcelFile;
            TranslationName = translationName;
        }

        public string QuestionnaireTitle { set; get; }
        public byte[] ContentAsExcelFile { set; get; }
        public string TranslationName { set; get; }
    }
}
