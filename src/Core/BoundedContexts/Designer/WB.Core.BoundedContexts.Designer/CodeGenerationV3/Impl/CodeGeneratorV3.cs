using WB.Core.BoundedContexts.Designer.CodeGenerationV2;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV3.Impl
{
    internal class CodeGeneratorV3 : CodeGeneratorV2, ICodeGeneratorV3
    {
        public CodeGeneratorV3(ICodeGenerationModelsFactoryV3 modelsFactory) : base(modelsFactory)
        {
        }
    }
}