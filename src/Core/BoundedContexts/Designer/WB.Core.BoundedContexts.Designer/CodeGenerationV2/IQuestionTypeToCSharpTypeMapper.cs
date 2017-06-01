﻿using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2
{
    public interface IQuestionTypeToCSharpTypeMapper
    {
        string GetType(IQuestion question, ReadOnlyQuestionnaireDocument questionnaire);
        string GetVariablesCSharpType(VariableType variableType);
    }
}