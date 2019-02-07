using System;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IInterviewQuestionnaireReferenceStorage
    {
        QuestionnaireId GetQuestionnaireIdByInterviewId(Guid interviewId);

        void AddInterviewQuestionnaireReference(Guid interviewId, QuestionnaireId questionnaireId);
    }

    public class InterviewQuestionnaireReferenceStorage : IInterviewQuestionnaireReferenceStorage
    {
        public QuestionnaireId GetQuestionnaireIdByInterviewId(Guid interviewId)
        {
            throw new NotImplementedException();
        }

        public void AddInterviewQuestionnaireReference(Guid interviewId, QuestionnaireId questionnaireId)
        {
            throw new NotImplementedException();
        }
    }
}
