﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICategoriesExtractService
    {
        List<CategoriesRow> Extract(Stream file);
        byte[] GetTemplateFile(bool isCascading);
        byte[] GetAsFile(List<CategoriesItem> items, bool isCascading, bool hqImport);
    }
}
