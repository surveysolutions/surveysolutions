using System;
using System.Linq;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2;

class CodeGenerationLookupTableService : ICodeGenerationLookupTableService
{
    private readonly IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage;
    private readonly DesignerDbContext dbContext;

    public CodeGenerationLookupTableService(IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage,
        DesignerDbContext dbContext)
    {
        this.lookupTableContentStorage = lookupTableContentStorage;
        this.dbContext = dbContext;
    }

    public LookupTableContent? GetLookupTableContent(ReadOnlyQuestionnaireDocument questionnaire, Guid lookupTableId)
    {
        if (!questionnaire.LookupTables.ContainsKey(lookupTableId))
            throw new ArgumentException(string.Format(ExceptionMessages.LookupTableIsMissing, lookupTableId));

        var lookupTable = questionnaire.LookupTables[lookupTableId];
        if (lookupTable == null)
            throw new ArgumentException(string.Format(ExceptionMessages.LookupTableHasEmptyContent, lookupTableId));

        var anonymousQuestionnaires = dbContext.AnonymousQuestionnaires.
            FirstOrDefault(a => a.AnonymousQuestionnaireId == questionnaire.PublicKey && a.IsActive == true);
        var questionnaireId = anonymousQuestionnaires?.QuestionnaireId ?? questionnaire.PublicKey;
        var lookupTableStorageId = $"{questionnaireId.FormatGuid()}-{lookupTableId.FormatGuid()}";

        var lookupTableContent = this.lookupTableContentStorage.GetById(lookupTableStorageId);
        return lookupTableContent;
    }
}