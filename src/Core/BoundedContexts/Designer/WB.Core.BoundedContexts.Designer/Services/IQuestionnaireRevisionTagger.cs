using Main.Core.Documents;
using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireRevisionMetadataUpdater
    {
        void LogInHistoryImportQuestionnaireToHq(QuestionnaireDocument questionnaireDocument, string userAgent, Guid userId);
        void UpdateQuestionnaireMetadata(Guid questionnaire, int revision, QuestionnaireRevisionMetaDataUpdate metaData);
    }
}
