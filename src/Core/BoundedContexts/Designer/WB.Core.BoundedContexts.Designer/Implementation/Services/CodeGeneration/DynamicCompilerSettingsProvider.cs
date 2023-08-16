﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class DynamicCompilerSettingsProvider : IDynamicCompilerSettingsProvider
    {
        private readonly string[] assemblies =
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
            "System.Text.RegularExpressions",
            "System.Threading"
        };

        private List<MetadataReference>? cachedReferences = null;
        public List<MetadataReference> GetAssembliesToReference()
        {
            return cachedReferences ??= GetAssembliesToReferenceImpl();
        }

        private List<MetadataReference> GetAssembliesToReferenceImpl()
        {
            var references = new List<MetadataReference>();
            var designerContextModuleAssembly = Assembly.GetAssembly(typeof(DesignerBoundedContextModule));
            if (designerContextModuleAssembly == null) throw new InvalidOperationException("Assembly is null.");
            
            foreach (var assembly in assemblies)
            {
                var stream = designerContextModuleAssembly.GetManifestResourceStream($"WB.Core.BoundedContexts.Designer.{assembly}.dll");
                if (stream == null)
                {
                    throw new Exception($"Cannot find {assembly} in WB.Core.BoundedContexts.Designer.ReferencedAssemblies");
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
