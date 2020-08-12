using System;

namespace WB.Services.Export.Questionnaire.Services
{
    public interface IQuestionnaireStorageCache 
    {
        bool TryGetValue(QuestionnaireId key, Guid? translation, out QuestionnaireDocument? document);
        void Remove(QuestionnaireId key, Guid? translation);
        void Set(QuestionnaireId key, Guid? translation, QuestionnaireDocument questionnaire);
    }
}
