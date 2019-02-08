using System;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewQuestionnaireReferenceNode
    {
        public string QuestionnaireId { get; set; }
        public Guid InterviewId { get; set; }
    }
}