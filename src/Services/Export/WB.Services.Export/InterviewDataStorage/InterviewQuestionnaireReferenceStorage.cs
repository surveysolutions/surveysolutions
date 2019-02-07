using System;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Storage;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IInterviewQuestionnaireReferenceStorage
    {
        QuestionnaireId GetQuestionnaireIdByInterviewId(Guid interviewId);

        void AddInterviewQuestionnaireReference(Guid interviewId, QuestionnaireId questionnaireId);

        void RemoveInterviewQuestionnaireReference(Guid interviewId);
    }

    public class InterviewQuestionnaireReferenceStorage : IInterviewQuestionnaireReferenceStorage
    {
        private readonly ISession session;

        public InterviewQuestionnaireReferenceStorage(ISession session)
        {
            this.session = session;
        }

        public QuestionnaireId GetQuestionnaireIdByInterviewId(Guid interviewId)
        {
            throw new NotImplementedException();
        }

        public void AddInterviewQuestionnaireReference(Guid interviewId, QuestionnaireId questionnaireId)
        {
            throw new NotImplementedException();
        }

        public void RemoveInterviewQuestionnaireReference(Guid interviewId)
        {
            throw new NotImplementedException();
        }
    }
}
