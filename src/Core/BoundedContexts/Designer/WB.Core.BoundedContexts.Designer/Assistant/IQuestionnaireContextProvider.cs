using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Assistant;

public interface IQuestionnaireContextProvider
{
    string GetQuestionnaireContext(Guid questionnaireId, Guid entityId, List<string>? loadGroups = null);
}
