using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Translations;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal interface ICategoriesVerifier
    {
        ImportValidationError? Verify(CategoriesRow row, CategoriesHeaderMap headers);
        void VerifyAll(List<CategoriesRow> rows, CategoriesHeaderMap headers);
    }
}
