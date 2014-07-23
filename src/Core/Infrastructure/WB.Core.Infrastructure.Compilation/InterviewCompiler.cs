using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.Infrastructure.Compilation
{
    public class RoslynCompiler : IDynamicCompiler
    {
        private const string ProfileToBuild = "Profile24";

        //private readonly string portableAttribute;
        private readonly string portableAssembliesPath;

        //private readonly string[] usingItems = new[] { "System.Runtime.Versioning", "WB.Core.SharedKernels.ExpressionProcessing" };

        private readonly string[] defaultReferencedPortableAssemblies = new[] { "System.dll", "System.Core.dll", "mscorlib.dll" };


        public RoslynCompiler()
        {
            //should be resolve outside
            this.portableAssembliesPath =
                String.Format("C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.0\\Profile\\{0}",
                    ProfileToBuild);
        }

        public EmitResult GenerateAssemblyAsString(Guid templateId, string classCode, string[] referencedPortableAssemblies, out string generatedAssembly)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(classCode);

            var metadataFileReference = this.defaultReferencedPortableAssemblies.Select(defaultReferencedPortableAssembly => new MetadataFileReference(Path.Combine(this.portableAssembliesPath, defaultReferencedPortableAssembly))).ToList();
            metadataFileReference.AddRange(referencedPortableAssemblies.Select(defaultReferencedPortableAssembly => new MetadataFileReference(Path.Combine(this.portableAssembliesPath, defaultReferencedPortableAssembly))));
            metadataFileReference.Add(new MetadataFileReference(typeof (IInterviewEvaluator).Assembly.Location));
            
            var compilation = CSharpCompilation.Create(
                String.Format("rules-{0}.dll", templateId),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] { tree },
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