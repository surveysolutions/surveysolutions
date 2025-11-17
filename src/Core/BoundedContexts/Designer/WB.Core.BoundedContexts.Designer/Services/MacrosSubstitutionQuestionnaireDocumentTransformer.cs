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
            IMacrosSubstitutionService macrosSubstitutionService)
        {
            this.macrosSubstitutionService = macrosSubstitutionService;
        }

        public QuestionnaireDocument TransformInPlace(QuestionnaireDocument document)
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
