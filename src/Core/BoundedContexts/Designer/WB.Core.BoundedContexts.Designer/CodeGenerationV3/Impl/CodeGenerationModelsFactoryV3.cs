using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV3.Impl
{
    internal class CodeGenerationModelsFactoryV3 : CodeGenerationModelsFactory, 
        ICodeGenerationModelsFactoryV3
    {
        public CodeGenerationModelsFactoryV3(IMacrosSubstitutionService macrosSubstitutionService, 
            ILookupTableService lookupTableService,
            IQuestionTypeToCSharpTypeMapperV3 questionTypeMapper) : 
            base(macrosSubstitutionService, lookupTableService, questionTypeMapper)
        {
        }
    }
}