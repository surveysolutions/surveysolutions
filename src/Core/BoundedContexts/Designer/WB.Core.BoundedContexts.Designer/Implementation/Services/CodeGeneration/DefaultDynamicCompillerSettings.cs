using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class DefaultDynamicCompillerSettings : IDynamicCompilerSettings
    {
        public string PortableAssembliesPath { get; set; }
        public IEnumerable<string> DefaultReferencedPortableAssemblies { get; set; }
    }
}
