using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IMacrosSubstitutionService
    {
        string InlineMacros(string? expression, IEnumerable<Macro> macros);
    }
}
