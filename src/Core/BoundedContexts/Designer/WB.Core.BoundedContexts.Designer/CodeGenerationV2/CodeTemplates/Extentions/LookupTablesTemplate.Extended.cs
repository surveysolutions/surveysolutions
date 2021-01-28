using System.Collections.Generic;

using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    public partial class LookupTablesTemplate
    {
        public LookupTablesTemplate(List<LookupTableTemplateModel> lookupTables, string className)
        {
            this.LookupTables = lookupTables;
            this.ClassName = className;
        }

        public List<LookupTableTemplateModel> LookupTables { get; }
        public string ClassName { get; }
    }
}
