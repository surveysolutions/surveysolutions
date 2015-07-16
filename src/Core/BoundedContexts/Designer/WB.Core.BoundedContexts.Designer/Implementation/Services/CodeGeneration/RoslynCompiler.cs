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
using WB.Core.SharedKernels.DataCollection;

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
            string[] referencedPortableAssemblies,
            IDynamicCompilerSettings settings,
            out string generatedAssembly)
        {
            IEnumerable<SyntaxTree> syntaxTrees = generatedClasses.Select(
                    generatedClass => SyntaxFactory.ParseSyntaxTree(generatedClass.Value, path: generatedClass.Key))
                    .ToArray();

            var metadataReferences = new List<PortableExecutableReference>
            {
                AssemblyMetadata.CreateFromFile(typeof (Identity).Assembly.Location).GetReference()
            };

            metadataReferences.AddRange(this.GetMetadataRefereces(settings.DefaultReferencedPortableAssemblies, settings.PortableAssembliesPath));
            metadataReferences.AddRange(this.GetMetadataRefereces(referencedPortableAssemblies, settings.PortableAssembliesPath));
            
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

        IEnumerable<PortableExecutableReference> GetMetadataRefereces(
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