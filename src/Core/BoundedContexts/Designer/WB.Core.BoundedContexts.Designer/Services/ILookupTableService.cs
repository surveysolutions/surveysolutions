using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ILookupTableService
    {
        void SaveLookupTableContent(Guid questionnaireId, Guid lookupTableId, string fileContent);
        void DeleteAllByQuestionnaireId(Guid questionnaireId);
        LookupTableContent? GetLookupTableContent(Guid questionnaireId, Guid lookupTableId);
        LookupTableContentFile? GetLookupTableContentFile(QuestionnaireRevision questionnaireId, Guid lookupTableId);
        Dictionary<Guid, string> GetQuestionnairesLookupTables(Guid questionnaireId);
        Dictionary<Guid, string> GetQuestionnairesLookupTables(QuestionnaireRevision questionnaireId);
        void CloneLookupTable(Guid sourceQuestionnaireId, Guid sourceTableId, Guid newQuestionnaireId, Guid newLookupTableId);
        bool IsLookupTableEmpty(Guid questionnaireId, Guid tableId, string? lookupTableName);
    }
}
