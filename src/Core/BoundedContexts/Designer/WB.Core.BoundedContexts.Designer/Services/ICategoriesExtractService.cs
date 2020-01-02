using System.Collections.Generic;
using System.IO;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal interface ICategoriesExtractService
    {
        List<CategoriesRow> Extract(Stream file);
    }
}
