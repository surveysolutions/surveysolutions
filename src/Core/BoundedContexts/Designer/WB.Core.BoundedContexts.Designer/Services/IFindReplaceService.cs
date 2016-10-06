using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IFindReplaceService
    {
        IEnumerable<QuestionnaireNodeReference> FindAll(Guid questionnaireId, string searchFor);
    }
}