using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class RoslynCompiler : IDynamicCompiler
    {
        public EmitResult TryGenerateAssemblyAsStringAndEmitResult(
            Guid templateId,
            Dictionary<string, string> generatedClasses,
            IEnumerable<MetadataReference> referencedPortableAssemblies,
            out string generatedAssembly)
        {
            generatedAssembly = string.Empty;

            IEnumerable<SyntaxTree> syntaxTrees = generatedClasses
                .Where(g => !g.Key.StartsWith("RESOURCE__", StringComparison.OrdinalIgnoreCase))
                .Select(
                    generatedClass => SyntaxFactory.ParseSyntaxTree(generatedClass.Value, path: generatedClass.Key))
                    .ToArray();

            var metadataReferences = new List<MetadataReference>();
            metadataReferences.AddRange(referencedPortableAssemblies);

            CSharpCompilation compilation = CreateCompilation(templateId, syntaxTrees, metadataReferences);

            using var stream = new MemoryStream();

            var resources = generatedClasses.Where(g => g.Key.StartsWith("RESOURCE__", StringComparison.OrdinalIgnoreCase)).Select(kv =>
                new ResourceDescription(kv.Key,
                    () => new MemoryStream(Encoding.UTF8.GetBytes(kv.Value)), true));

            EmitResult compileResult = compilation.Emit(stream, manifestResources: resources);
            
            if (compileResult.Success)
            {
                stream.Position = 0;
                generatedAssembly = Convert.ToBase64String(stream.ToArray());
            }

            return compileResult;
        }

        static CSharpCompilation CreateCompilation(Guid templateId, IEnumerable<SyntaxTree> syntaxTrees, List<MetadataReference> metadataReferences)
        {
            return CSharpCompilation.Create(
                String.Format("rules-{0}-{1}.dll", templateId.FormatGuid(), Guid.NewGuid().FormatGuid()),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, 
                    checkOverflow: true, 
                    optimizationLevel: OptimizationLevel.Release, 
                    warningLevel: 1,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default),
                syntaxTrees: syntaxTrees,
                references: metadataReferences);
        }
    }
}
