using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class CodeSecurityChecker
    {
        private static readonly HashSet<string> AllowedNamespaces = new HashSet<string>
        {
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Linq.Queryable",
            "System.Text.RegularExpressions"
        };

        private static readonly HashSet<string> ForbiddenClassesFromSystemNamespace = new HashSet<string>
        {
            "System.Activator", 
            "System.AppContext", 
            "System.AppDomain", 
            "System.Console", 
            "System.Environment", 
            "System.GC"
        };

        public IEnumerable<string> FindForbiddenClassesUsage(SyntaxTree syntaxTree, CSharpCompilation compilation)
        {
            var allUsedTypes = FindUsedTypes(syntaxTree, compilation);

            HashSet<string> foundForbiddenTypes = new HashSet<string>();
            foreach (var namedTypeSymbol in allUsedTypes.Where(x => x.ContainingAssembly?.Name != compilation.AssemblyName))
            {
                var containingNamespace = namedTypeSymbol.ContainingNamespace.ToString();
                if(containingNamespace == null) continue;
                var symbol = namedTypeSymbol.ToString();
                if(symbol == null) continue;
                
                if (namedTypeSymbol.Kind == SymbolKind.NamedType &&
                    string.Compare(containingNamespace, "System", StringComparison.InvariantCulture) == 0 &&
                    ForbiddenClassesFromSystemNamespace.Contains(symbol))
                {
                    
                    if (!foundForbiddenTypes.Contains(symbol))
                    {
                        foundForbiddenTypes.Add(symbol);
                        yield return symbol;
                    }
                    continue;
                }
                
                if (!AllowedNamespaces.Contains(containingNamespace))
                {
                    if (!containingNamespace.StartsWith("WB.Core.SharedKernels.DataCollection", StringComparison.InvariantCulture))
                    {
                        if (!foundForbiddenTypes.Contains(symbol))
                        {
                            foundForbiddenTypes.Add(symbol);
                            yield return symbol;
                        }
                    }
                }
            }
        }

        // https://stackoverflow.com/a/29178633/72174
        static IEnumerable<INamedTypeSymbol> FindUsedTypes(SyntaxTree tree, CSharpCompilation compilation)
        {
            var root = tree.GetRoot();
            var st = root.SyntaxTree;
            var sm = compilation.GetSemanticModel(st);
            var syntaxNodes = root.DescendantNodes();

            foreach (var syntaxNode in syntaxNodes)
            {
                // IdentifierNameSyntax:
                //  - var keyword
                //  - identifiers of any kind (including type names)
                if (syntaxNode is IdentifierNameSyntax identifierNameSyntax)
                {
                    var symbol = sm.GetSymbolInfo(identifierNameSyntax).Symbol;
                    if (symbol is INamedTypeSymbol namedTypeSymbol)
                        yield return namedTypeSymbol;
                }
                
                // ExpressionSyntax:
                //  - method calls
                //  - property uses
                //  - field uses
                //  - all kinds of composite expressions
                if (syntaxNode is ExpressionSyntax expressionSyntax)
                {
                    var type = sm.GetTypeInfo(expressionSyntax).Type;
                    if (type is INamedTypeSymbol namedTypeSymbol)
                        yield return namedTypeSymbol;
                }
            }
        }
    }
}
