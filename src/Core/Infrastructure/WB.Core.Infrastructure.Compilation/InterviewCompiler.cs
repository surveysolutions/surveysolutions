using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.Infrastructure.BaseStructures;

namespace WB.Core.Infrastructure.Compilation
{
    public class InterviewCompiler : IDynamicCompiler
    {
        private const string ProfileToBuild = "Profile7";

        private readonly string portableAttribute;
        private readonly string portableAssembliesPath;

        public static string TestClass = @"
            public class InterviewEvaluator : IInterviewEvaluator
            {
                public static object Evaluate()
                {
                    return 2+2*2;
                }

                public int Test()
                {
                    return 40 + 2;
                }
 
            }";

        private readonly string[] usingItems = new[] { "using System.Runtime.Versioning;", "using WB.Core.Infrastructure.BaseStructures;" };


        public InterviewCompiler()
        {
            //should be resolve
            portableAssembliesPath = String.Format("C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.5\\Profile\\{0}", ProfileToBuild);

            portableAttribute = String.Format(
                @"[assembly: TargetFramework("".NETPortable,Version=v4.5,Profile={0}"", FrameworkDisplayName="".NET Portable Subset"")]", ProfileToBuild);
        }

        public EmitResult GenerateAssemblyAsString(Guid templateId, string classCode, out string generatedAssembly)
        {
            generatedAssembly = string.Empty;

            var builder = new StringBuilder();

            foreach (var usingitem in this.usingItems)
            {
                builder.AppendLine(usingitem);
            }

            builder.AppendLine(this.portableAttribute);

            builder.Append(classCode);

            var tree = SyntaxFactory.ParseSyntaxTree(builder.ToString());

            var metadataFileReference = new List<MetadataFileReference>
            {
                new MetadataFileReference(Path.Combine(this.portableAssembliesPath, "System.Runtime.dll")),
                new MetadataFileReference(Path.Combine(this.portableAssembliesPath, "mscorlib.dll")),
                new MetadataFileReference(typeof(IInterviewEvaluator).Assembly.Location)
            };

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

        private string GenerateClassCode()
        {
            //generate template 
            return TestClass;
        }
    }
}