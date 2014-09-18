using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class RoslynExpressionAnalyser
    {
        public IEnumerable<string> ExtractVariables(string expression)
        {
            string code = string.Format("internal partial class X {{ private bool M() {{ return ({0}); }} }} ", expression);

            return SyntaxFactory
                .ParseSyntaxTree(code).GetRoot().ChildNodesAndTokens().TreeToEnumerable(_ => _.ChildNodesAndTokens())
                .Where(IsIdentifierToken)
                .Select(token => token.ToString());
        }

        private static bool IsIdentifierToken(SyntaxNodeOrToken nodeOrToken)
        {
            return nodeOrToken.IsToken
                && nodeOrToken.AsToken().CSharpKind() == SyntaxKind.IdentifierToken
                && nodeOrToken.Parent.CSharpKind() == SyntaxKind.IdentifierName;
        }
    }
}