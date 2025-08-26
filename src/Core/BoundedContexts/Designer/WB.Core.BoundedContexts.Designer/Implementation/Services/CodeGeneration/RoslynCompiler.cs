﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class RoslynCompiler : IDynamicCompiler
    {
        public GenerationResult TryGenerateAssemblyAsStringAndEmitResult(
            Guid templateId,
            Dictionary<string, string> generatedClasses,
            IEnumerable<MetadataReference> referencedPortableAssemblies,
            out string generatedAssembly)
        {
            generatedAssembly = string.Empty;

            SyntaxTree[] syntaxTrees = generatedClasses.Select(
                    generatedClass => 
                        SyntaxFactory.ParseSyntaxTree(generatedClass.Value, 
                            path: generatedClass.Key, 
                            options: new CSharpParseOptions(documentationMode:DocumentationMode.None)))
                    .ToArray();

            var metadataReferences = new List<MetadataReference>();
            metadataReferences.AddRange(referencedPortableAssemblies);
            
            CSharpCompilation compilation = CreateCompilation(templateId, syntaxTrees, metadataReferences);
            EmitResult compileResult;
            
            var foundUsages = new CodeSecurityChecker().FindForbiddenClassesUsage(syntaxTrees, compilation);
            if (foundUsages.Any())
            {
                return new GenerationResult(
                    success: false,
                    diagnostics: [
                        Diagnostic.Create(
                            new DiagnosticDescriptor("SEC001", "Forbidden class usage", 
                            "Usage of forbidden class", "Security", DiagnosticSeverity.Error, true), 
                            Location.None)
                    ]
                );
            }
            
            using (var stream = new MemoryStream())
            {
                compileResult = compilation.Emit(stream);

                if (compileResult.Success)
                {
                    stream.Position = 0;
                    generatedAssembly = Convert.ToBase64String(stream.ToArray());
                }

            }

            return new GenerationResult(compileResult.Success, compileResult.Diagnostics);
        }

        static CSharpCompilation CreateCompilation(Guid templateId, IEnumerable<SyntaxTree> syntaxTrees, List<MetadataReference> metadataReferences)
        {
            return CSharpCompilation.Create(
                String.Format("rules-{0}-{1}.dll", templateId.FormatGuid(), Guid.NewGuid().FormatGuid()),
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary, 
                    checkOverflow: true, 
                    optimizationLevel: OptimizationLevel.Release, 
                    warningLevel: 1,
                    allowUnsafe: false,
                    concurrentBuild: true,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default),
                syntaxTrees: syntaxTrees,
                references: metadataReferences);
        }
    }
}
