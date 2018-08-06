using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class DynamicCompilerSettingsProvider : IDynamicCompilerSettingsProvider
    {
        private readonly List<string> assemblies = new List<string>
        {
            "System.Collections",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Runtime",
            "System.Runtime.Extensions",
            "System.Runtime.Numerics",
            "System.Text.RegularExpressions"
        };

        public List<MetadataReference> GetAssembliesToReference()
        {
            var references = new List<MetadataReference>();

            foreach (var assembly in assemblies)
            {
                var assemblyMetadata =
                    AssemblyMetadata.CreateFromStream(Assembly.GetAssembly(typeof(DesignerBoundedContextModule))
                        .GetManifestResourceStream($"WB.Core.BoundedContexts.Designer.ReferencedAssemblies.{assembly}.dll"));

                PortableExecutableReference reference = assemblyMetadata.GetReference();
                references.Add(reference);
            }
            
            references.Add(AssemblyMetadata.CreateFromFile(typeof(Identity).Assembly.Location).GetReference());
            return references;
        }
    }
}
