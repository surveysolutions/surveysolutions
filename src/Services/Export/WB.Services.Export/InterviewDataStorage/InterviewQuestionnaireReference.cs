using System;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewQuestionnaireReference
    {
        public Guid InterviewId { get; set; }
        public QuestionnaireId QuestionnaireId { get; set; }
    }
}
