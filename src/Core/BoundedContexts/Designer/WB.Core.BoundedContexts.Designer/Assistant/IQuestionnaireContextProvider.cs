using System;

namespace WB.Core.BoundedContexts.Designer.Assistant;

public interface IQuestionnaireContextProvider
{
    string GetQuestionnaireContext(Guid questionnaireId);
}
