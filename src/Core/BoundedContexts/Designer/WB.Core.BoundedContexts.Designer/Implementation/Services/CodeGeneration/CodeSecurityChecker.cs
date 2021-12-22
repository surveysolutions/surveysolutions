using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class CodeSecurityChecker
    {
        private static readonly List<string> AllowedNamespaces = new List<string>
        {
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Linq.Queryable",
            "System.Text.RegularExpressions"
        };

        private static readonly List<string> ForbiddenClassesFromSystemNamespace = new List<string>
        {
            "System.Activator", "System.AppContext", "System.AppDomain", "System.Console", "System.Environment", "System.GC"
        };

        public List<string> FindForbiddenClassesUsage(SyntaxTree syntaxTree, CSharpCompilation compilation)
        {
            var allUsedTypes = FindUsedTypes(syntaxTree, compilation);

            HashSet<string> foundForbiddenTypes = new HashSet<string>();
            foreach (var namedTypeSymbol in allUsedTypes.Where(x => x.ContainingAssembly?.Name != compilation.AssemblyName))
            {
                var containingNamespace = namedTypeSymbol.ContainingNamespace.ToString();

                if (containingNamespace == "System" &&
                    namedTypeSymbol.Kind == SymbolKind.NamedType &&
                    ForbiddenClassesFromSystemNamespace.Contains(namedTypeSymbol.ToString()))
                {
                    foundForbiddenTypes.Add(namedTypeSymbol.ToString());
                    continue;
                }

                if (!AllowedNamespaces.Contains(containingNamespace) && !containingNamespace.StartsWith("WB.Core.SharedKernels.DataCollection"))
                {
                    foundForbiddenTypes.Add(namedTypeSymbol.ToString());
                }
            }

            return foundForbiddenTypes.ToList();
        }

        // https://stackoverflow.com/a/29178633/72174
        static List<INamedTypeSymbol> FindUsedTypes(SyntaxTree tree, CSharpCompilation compilation)
        {
                var root = tree.GetRoot();
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
            return namedTypeSymbols;
        }
    }
}
