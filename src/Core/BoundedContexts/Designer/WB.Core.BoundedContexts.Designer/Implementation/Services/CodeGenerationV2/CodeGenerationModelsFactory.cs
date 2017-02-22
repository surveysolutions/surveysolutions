using System;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    public class CodeGenerationModelsFactory : ICodeGenerationModelsFactory
    {
        public CodeGenerationModel CreateModel(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var codeGenerationModel = new CodeGenerationModel
            {
                Id = questionnaire.PublicKey,
                ClassName = $"{CodeGeneratorV2.InterviewExpressionStatePrefix}_{Guid.NewGuid().FormatGuid()}",
            };
            return codeGenerationModel;
        }
    }
}