using System;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnireHistoryVersionsService
    {
        QuestionnaireDocument GetByHistoryVersion(Guid historyReferanceId);
        string GetResultingQuestionnaireDocument(QuestionnaireDocument questionnaireDocument);
    }
}