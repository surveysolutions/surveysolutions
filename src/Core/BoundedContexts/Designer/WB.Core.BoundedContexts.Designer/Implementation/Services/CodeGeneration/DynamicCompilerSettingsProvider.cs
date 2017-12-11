using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class DynamicCompilerSettingsProvider : IDynamicCompilerSettingsProvider
    {
        private static readonly string[] StaticAssembliesRefs =
        {
            //"System.dll",
            //"System.Core.dll",
            //"System.Runtime.dll",
            //"System.Collections.dll",
            //"System.Linq.dll",
            //"System.Linq.Expressions.dll",
            //"System.Linq.Queryable.dll",
            //"mscorlib.dll",
            //"System.Runtime.Extensions.dll",
            //"System.Text.RegularExpressions.dll",
            "netstandard.dll"
        };

        public List<MetadataReference> GetAssembliesToReference(int apiVersion)
        {
            var references = new List<MetadataReference>();
            references.Add(AssemblyMetadata.CreateFromFile(typeof(Identity).Assembly.Location).GetReference());
            var assembly = typeof(DesignerBoundedContextModule).Assembly;
            foreach (var staticAssembliesRef in StaticAssembliesRefs)
            {
                var referenceResource = assembly.GetManifestResourceStream("WB.Core.BoundedContexts.Designer.netStandardRefs." + staticAssembliesRef);
                AssemblyMetadata assemblyMetadata = AssemblyMetadata.CreateFromStream(referenceResource);
                references.Add(assemblyMetadata.GetReference());
            }

            return references;
        }
    }
}