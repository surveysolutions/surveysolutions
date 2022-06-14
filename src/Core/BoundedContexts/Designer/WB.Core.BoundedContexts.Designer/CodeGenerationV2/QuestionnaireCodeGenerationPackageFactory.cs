using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2;

class QuestionnaireCodeGenerationPackageFactory : IQuestionnaireCodeGenerationPackageFactory
{
    private readonly ILookupTableService lookupTableService;

    public QuestionnaireCodeGenerationPackageFactory(ILookupTableService lookupTableService)
    {
        this.lookupTableService = lookupTableService;
    }

    public QuestionnaireCodeGenerationPackage Generate(QuestionnaireDocument document, Guid? originalQuestionnaireId = null)
    {
        var questionnaireId = originalQuestionnaireId ?? document.PublicKey;
        var lookupTables = GetLookupTables(document, questionnaireId);
        return new QuestionnaireCodeGenerationPackage(document, lookupTables);
    }
    
    private Dictionary<Guid, LookupTableContent> GetLookupTables(QuestionnaireDocument questionnaire, Guid questionnaireId)
    {
        Dictionary<Guid, LookupTableContent> tables = new();
        foreach (var table in questionnaire.LookupTables)
        {
            var lookupTableContent = lookupTableService.GetLookupTableContent(questionnaireId, table.Key);
            if (lookupTableContent == null)
                throw new InvalidOperationException("Lookup table is empty.");
            tables.Add(table.Key, lookupTableContent);
        }

        return tables;
    }
}