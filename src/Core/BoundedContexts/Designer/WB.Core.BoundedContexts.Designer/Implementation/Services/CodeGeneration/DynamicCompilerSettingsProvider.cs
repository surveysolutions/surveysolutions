using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class DynamicCompilerSettingsProvider : IDynamicCompilerSettingsProvider
    {
        private readonly List<string> assemblies = new()
        {
            "System.Globalization",
            "System.Reflection",
            "System.IO",
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
            var designerContextModule = Assembly.GetAssembly(typeof(DesignerBoundedContextModule));
            foreach (var assembly in assemblies)
            {
                var stream =
                    designerContextModule.GetManifestResourceStream(
                        $"WB.Core.BoundedContexts.Designer.{assembly}.dll");
                if (stream == null)
                {
                    throw new Exception(
                        $"Cannot find {assembly} in WB.Core.BoundedContexts.Designer.ReferencedAssemblies");
                }
                var assemblyMetadata = AssemblyMetadata.CreateFromStream(stream);

                PortableExecutableReference reference = assemblyMetadata.GetReference();
                references.Add(reference);
            }
            
            references.Add(AssemblyMetadata.CreateFromFile(typeof(Identity).Assembly.Location).GetReference());
            return references;
        }
    }
}
