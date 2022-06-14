using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2;

public class QuestionnaireCodeGenerationPackage
{
    public QuestionnaireCodeGenerationPackage(QuestionnaireDocument questionnaireDocument, Dictionary<Guid, LookupTableContent> lookupTables)
    {
        QuestionnaireDocument = new ReadOnlyQuestionnaireDocumentWithCache(questionnaireDocument);
        LookupTables = lookupTables;
    }

    public ReadOnlyQuestionnaireDocumentWithCache QuestionnaireDocument { get; }
    public Dictionary<Guid, LookupTableContent> LookupTables { get; }
}