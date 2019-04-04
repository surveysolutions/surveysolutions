namespace WB.Services.Export.Questionnaire.Services
{
    public interface IQuestionnaireStorageCache 
    {
        bool TryGetValue(QuestionnaireId key, out QuestionnaireDocument document);
        void Remove(QuestionnaireId key);
        void Set(QuestionnaireId key, QuestionnaireDocument questionnaire);
    }
}
