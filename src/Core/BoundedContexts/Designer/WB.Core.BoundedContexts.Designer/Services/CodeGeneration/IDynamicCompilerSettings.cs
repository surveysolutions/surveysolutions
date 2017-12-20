using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Services.CodeGeneration
{
    public interface IDynamicCompilerSettings
    {
        string Name { get; }
        string PortableAssembliesPath { get; }
        IEnumerable<string> DefaultReferencedPortableAssemblies { get; }
    }
}