using System;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2;

public interface IQuestionnaireCodeGenerationPackageFactory
{
    QuestionnaireCodeGenerationPackage Generate(QuestionnaireDocument document, Guid? originalQuestionnaireId = null);
}