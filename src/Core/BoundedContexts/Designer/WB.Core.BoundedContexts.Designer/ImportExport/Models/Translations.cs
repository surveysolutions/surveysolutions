using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models;

public class Translations
{
    public Guid? DefaultTranslation { get; set; }
    public string? OriginalDisplayName { get; set; }

    public List<Translation> Items { get; set; } = new List<Translation>();
}