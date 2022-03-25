using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models;

public class Translations
{
    public string? DefaultTranslation { get; set; }
    public string? DefaultTranslationDisplayName { get; set; }

    public List<Translation> Items { get; set; } = new List<Translation>();
}