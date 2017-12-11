using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            IEnumerable<SyntaxTree> syntaxTrees = generatedClasses.Select(
                    generatedClass => SyntaxFactory.ParseSyntaxTree(generatedClass.Value, path: generatedClass.Key))
                    .ToArray();

            var metadataReferences = new List<MetadataReference>();
            metadataReferences.AddRange(referencedPortableAssemblies);
            
            CSharpCompilation compilation = CreateCompilation(templateId, syntaxTrees, metadataReferences);
            //ValidateUsedTypes(syntaxTrees, compilation);
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

        private static List<string> allowedNamespaces = new List<string>
        {
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Linq.Queryable",
            "System.Text.RegularExpressions"
        };

        /// <summary>
        /// https://stackoverflow.com/a/29178633/72174
        /// </summary>
        static void ValidateUsedTypes(IEnumerable<SyntaxTree> trees, CSharpCompilation compilation)
        {
            foreach (var syntaxTree in trees)
            {
                var root = syntaxTree.GetRoot();
                var nodes = root.DescendantNodes(n => true);

                var st = root.SyntaxTree;
                var sm = compilation.GetSemanticModel(st);
                List<INamedTypeSymbol> namedTypeSymbols = new List<INamedTypeSymbol>();

                if (nodes != null)
                {
                    var syntaxNodes = nodes as SyntaxNode[] ?? nodes.ToArray();

                    // IdentifierNameSyntax:
                    //  - var keyword
                    //  - identifiers of any kind (including type names)
                    var namedTypes = syntaxNodes
                        .OfType<IdentifierNameSyntax>()
                        .Select(id => sm.GetSymbolInfo(id).Symbol)
                        .OfType<INamedTypeSymbol>();


                    namedTypeSymbols.AddRange(namedTypes);

                    // ExpressionSyntax:
                    //  - method calls
                    //  - property uses
                    //  - field uses
                    //  - all kinds of composite expressions
                    var expressionTypes = syntaxNodes
                        .OfType<ExpressionSyntax>()
                        .Select(ma => sm.GetTypeInfo(ma).Type)
                        .OfType<INamedTypeSymbol>();

                    namedTypeSymbols.AddRange(expressionTypes);
                }

                foreach (var namedTypeSymbol in namedTypeSymbols.Where(x => x.ContainingAssembly.Name != compilation.AssemblyName))
                {
                    var containingNamespace = namedTypeSymbol.ContainingNamespace.ToString();
                    if (!allowedNamespaces.Contains(containingNamespace) &&
                        !containingNamespace.StartsWith("WB.Core.SharedKernels.DataCollection"))
                    {
                        throw new Exception("Invalid Namespace");
                    }
                }
            }
        }
    }
}