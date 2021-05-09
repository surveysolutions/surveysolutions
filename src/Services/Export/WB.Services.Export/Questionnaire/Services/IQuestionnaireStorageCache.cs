using System;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Questionnaire.Services
{
    public interface IQuestionnaireStorageCache 
    {
        bool TryGetValue(QuestionnaireIdentity key, Guid? translation, out QuestionnaireDocument? document);
        void Remove(QuestionnaireIdentity key, Guid? translation);
        void Set(QuestionnaireIdentity key, Guid? translation, QuestionnaireDocument questionnaire);
    }
}
