using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IDatabaseSchemaService
    {
        void CreateQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument);
        bool TryDropQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument);
    }
}
