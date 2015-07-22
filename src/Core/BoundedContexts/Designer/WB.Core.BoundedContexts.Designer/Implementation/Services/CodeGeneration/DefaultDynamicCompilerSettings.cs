using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class DefaultDynamicCompilerSettings : IDynamicCompilerSettings
    {
        public string Name { get; set; }
        public string PortableAssembliesPath { get; set; }
        public IEnumerable<string> DefaultReferencedPortableAssemblies { get; set; }
    }
}
