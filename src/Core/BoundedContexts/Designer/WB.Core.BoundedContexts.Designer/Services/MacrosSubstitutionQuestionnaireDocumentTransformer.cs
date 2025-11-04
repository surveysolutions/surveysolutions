using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public class MacrosSubstitutionQuestionnaireDocumentTransformer : IQuestionnaireDocumentTransformer
    {
        private readonly IMacrosSubstitutionService macrosSubstitutionService;
        
        public MacrosSubstitutionQuestionnaireDocumentTransformer(
            IMacrosSubstitutionService macrosSubstitutionService,
            Guid? compileQuestionnaireId = null)
        {
            this.macrosSubstitutionService = macrosSubstitutionService;
        }

        public QuestionnaireDocument TransformDocument(QuestionnaireDocument document)
        {
            document.GetEntitiesByType<TextQuestion>()
                .ForEach(x => x.Mask =
                    x.Mask != null
                        ? macrosSubstitutionService.InlineMacros(x.Mask, document.Macros.Values)
                        : null);

            return document;
        }
    }
}
