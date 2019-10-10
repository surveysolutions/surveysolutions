using Main.Core.Documents;
using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireRevisionTagger
    {
        void LogInHistoryImportQuestionnaireToHq(QuestionnaireDocument questionnaireDocument, string userAgent, Guid userId);
        void UpdateQuestionnaireMetadata(Guid revision, QuestionnaireRevisionMetaDataUpdate metaData);
    }
}
