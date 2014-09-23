using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class RoslynExpressionProcessor : IExpressionProcessor
    {
        public bool IsSyntaxValid(string expression)
        {
            throw new NotImplementedException("Separate engine is now used for syntax validation.");
        }

        public IEnumerable<string> GetIdentifiersUsedInExpression(string expression)
        {
            string code = string.Format("class a {{ bool b() {{ return ({0}); }} }} ", expression);

            return SyntaxFactory
                .ParseSyntaxTree(code).GetRoot().ChildNodesAndTokens().TreeToEnumerable(_ => _.ChildNodesAndTokens())
                .Where(IsIdentifierToken)
                .Select(token => token.ToString())
                .Distinct();
        }

        public bool EvaluateBooleanExpression(string expression, Func<string, object> getValueForIdentifier)
        {
            throw new NotImplementedException("Separate engine is now used for evaluation.");
        }

        private static bool IsIdentifierToken(SyntaxNodeOrToken nodeOrToken)
        {
            return nodeOrToken.IsToken
                && nodeOrToken.AsToken().CSharpKind() == SyntaxKind.IdentifierToken
                && nodeOrToken.Parent.CSharpKind() == SyntaxKind.IdentifierName;
        }
    }
}