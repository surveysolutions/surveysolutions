using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class RoslynCompiler : IDynamicCompiler
    {
        private readonly IDynamicCompilerSettings compilerSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public RoslynCompiler(IDynamicCompilerSettings compilerSettings, IFileSystemAccessor fileSystemAccessor)
        {
            this.compilerSettings = compilerSettings;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        private IEnumerable<SyntaxTree> GetTrees(Dictionary<string, string> generatedClasses)
        {
            return generatedClasses.Select(generatedClass => SyntaxFactory.ParseSyntaxTree(generatedClass.Value, path: generatedClass.Key)).ToArray();
        }

        public EmitResult GenerateAssemblyAsString(Guid templateId, Dictionary<string, string> generatedClasses , string[] referencedPortableAssemblies,
            out string generatedAssembly)
        {
            var syntaxTrees = GetTrees(generatedClasses);

            var metadataFileReference =
                this.compilerSettings.DefaultReferencedPortableAssemblies.Select(
                    defaultReferencedPortableAssembly =>
                        new MetadataFileReference(
                            fileSystemAccessor.CombinePath(this.compilerSettings.PortableAssembliesPath,
                                defaultReferencedPortableAssembly))).ToList();
            metadataFileReference.AddRange(
                referencedPortableAssemblies.Select(
                    defaultReferencedPortableAssembly =>
                        new MetadataFileReference(
                            fileSystemAccessor.CombinePath(this.compilerSettings.PortableAssembliesPath,
                                defaultReferencedPortableAssembly))));
            metadataFileReference.Add(new MetadataFileReference(typeof(Identity).Assembly.Location));

            Guid uniqueAssemblySuffix = Guid.NewGuid();

            var compilation = CSharpCompilation.Create(
                String.Format("rules-{0}-{1}.dll", templateId, uniqueAssemblySuffix),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, checkOverflow: true),
                syntaxTrees: syntaxTrees,
                references: metadataFileReference);

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
    }
}