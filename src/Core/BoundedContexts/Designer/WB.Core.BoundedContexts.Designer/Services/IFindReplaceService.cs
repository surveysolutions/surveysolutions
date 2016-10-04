using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IFindReplaceService
    {
        IEnumerable<string> FindAll(Guid questionnaireId, string searchFor);
        void ReplaceAll(Guid questionnaireId, string searchFor, string replaceWith);
    }
}