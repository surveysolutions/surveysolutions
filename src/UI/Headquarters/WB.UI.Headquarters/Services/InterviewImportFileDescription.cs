using System.Collections.Generic;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Services
{
    public class InterviewImportFileDescription
    {
        public bool HasResponsibleColumn { get; set; }
        public List<InterviewImportColumn> ColumnsByPrefilledQuestions { get; set; }
        public string SampleId { get; set; }
        public List<InterviewImportPrefilledQuestion> PrefilledQuestions { get; set; }
        public string QuestionnaireTitle { get; set; }
        public string[] FileColumns { get; set; }
    }
}