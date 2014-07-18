using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.SharedKernels.ExpressionProcessing;

namespace WB.Core.Infrastructure.Compilation
{
    public class RoslynInterviewCompiler : IDynamicCompiler
    {
        private const string ProfileToBuild = "Profile24";

        private readonly string portableAttribute;
        private readonly string portableAssembliesPath;

        private readonly string[] usingItems = new[] { "System.Runtime.Versioning", "WB.Core.SharedKernels.ExpressionProcessing" };

        private readonly string[] defaultReferencedPortableAssemblies = new[] { "System.Core.dll", "mscorlib.dll" };


        public RoslynInterviewCompiler()
        {
            //should be resolve outside
            this.portableAssembliesPath =
                String.Format("C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.0\\Profile\\{0}",
                    ProfileToBuild);
        }

        public EmitResult GenerateAssemblyAsString(Guid templateId, string classCode, string[] aditionalNamespaces,
            string[] referencedPortableAssemblies, out string generatedAssembly)
        {
            generatedAssembly = string.Empty;

            var builder = new StringBuilder();

            builder.AppendLine(classCode);

            var tree = SyntaxFactory.ParseSyntaxTree(builder.ToString());

            var metadataFileReference = new List<MetadataFileReference>();

            foreach (var defaultReferencedPortableAssembly in this.defaultReferencedPortableAssemblies)
            {
                metadataFileReference.Add(
                    new MetadataFileReference(Path.Combine(this.portableAssembliesPath, defaultReferencedPortableAssembly)));
            }


            foreach (var defaultReferencedPortableAssembly in referencedPortableAssemblies)
            {
                metadataFileReference.Add(
                    new MetadataFileReference(Path.Combine(this.portableAssembliesPath, defaultReferencedPortableAssembly)));
            }

            metadataFileReference.Add(new MetadataFileReference(typeof (IInterviewEvaluator).Assembly.Location));


            var compilation = CSharpCompilation.Create(
                String.Format("rules-{0}.dll", templateId),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] { tree },
                references: metadataFileReference);

            EmitResult compileResult;

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