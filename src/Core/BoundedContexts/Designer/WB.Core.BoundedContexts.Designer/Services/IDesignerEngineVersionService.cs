using System;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IDesignerEngineVersionService
    {
        Version GetLatestSupportedVersion();

        Version GetQuestionnaireContentVersion(QuestionnaireDocument questionnaireDocument);

        bool IsClientVersionSupported(Version clientVersion);

        bool IsQuestionnaireDocumentSupportedByClientVersion(QuestionnaireDocument questionnaireDocument, Version clientVersion);
    }
}