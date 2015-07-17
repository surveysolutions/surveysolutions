using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class RoslynCompiler : IDynamicCompiler
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        public RoslynCompiler(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public EmitResult TryGenerateAssemblyAsStringAndEmitResult(
            Guid templateId,
            Dictionary<string, string> generatedClasses,
            PortableExecutableReference[] referencedPortableAssemblies,
            IDynamicCompilerSettings settings,
            out string generatedAssembly)
        {
            IEnumerable<SyntaxTree> syntaxTrees = generatedClasses.Select(
                    generatedClass => SyntaxFactory.ParseSyntaxTree(generatedClass.Value, path: generatedClass.Key))
                    .ToArray();

            var metadataReferences = new List<PortableExecutableReference>();

            metadataReferences.AddRange(this.GetMetadataReferences(settings.DefaultReferencedPortableAssemblies, settings.PortableAssembliesPath));
            metadataReferences.AddRange(referencedPortableAssemblies);
            
            CSharpCompilation compilation = CreateCompilation(templateId, syntaxTrees, metadataReferences);

            EmitResult compileResult;
            generatedAssembly = string.Empty;
           
            using (var stream = new MemoryStream())
            {
                compileResult = compilation.Emit(stream);

                if (compileResult.Success)
                {
                    stream.Position = 0;
                    generatedAssembly = Convert.ToBase64String(stream.ToArray());
                }
            }

            return compileResult;
        }

        static CSharpCompilation CreateCompilation(Guid templateId, IEnumerable<SyntaxTree> syntaxTrees, List<PortableExecutableReference> metadataReferences)
        {
            return CSharpCompilation.Create(
                String.Format("rules-{0}-{1}.dll", templateId.FormatGuid(), Guid.NewGuid().FormatGuid()),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, 
                    checkOverflow: true, 
                    optimizationLevel: OptimizationLevel.Release, 
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default),
                syntaxTrees: syntaxTrees,
                references: metadataReferences);
        }

        IEnumerable<PortableExecutableReference> GetMetadataReferences(
            IEnumerable<string> referencedPortableAssemblies,
            string portableAssembliesPath)
        {
            return referencedPortableAssemblies.Select(
                defaultReferencedPortableAssembly =>
                    AssemblyMetadata.CreateFromFile(
                        this.fileSystemAccessor.CombinePath(
                            portableAssembliesPath,
                            defaultReferencedPortableAssembly
                        )
                ).GetReference()
            );
        }
    }
}