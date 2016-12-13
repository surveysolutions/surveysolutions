
namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public class QuestionnaireImportResult
    {
        public QuestionnaireImportResult()
        {
            this.IsSuccess = true;
        }
        public bool IsSuccess { get; set; }
        public string QuestionnaireTitle { get; set; }
        public string ImportError { get; set; }
    }
}