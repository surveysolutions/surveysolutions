using System;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2;

public interface ICodeGenerationLookupTableService
{
    LookupTableContent? GetLookupTableContent(ReadOnlyQuestionnaireDocument questionnaire, Guid lookupTableId);
}