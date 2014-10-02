using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                .Where(identifierToken => !IsFunction(identifierToken))
                .Where(identifierToken => !IsConstructorCall(identifierToken))
                .Where(identifierToken => !IsPropertyOrMethod(identifierToken))
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
                && nodeOrToken.CSharpKind() == SyntaxKind.IdentifierToken
                && nodeOrToken.Parent.CSharpKind() == SyntaxKind.IdentifierName;
        }

        private static bool IsFunction(SyntaxNodeOrToken identifierToken)
        {
            return identifierToken.Parent.Parent is InvocationExpressionSyntax;
        }

        private static bool IsConstructorCall(SyntaxNodeOrToken identifierToken)
        {
            return identifierToken.Parent.Parent is ObjectCreationExpressionSyntax;
        }

        private static bool IsPropertyOrMethod(SyntaxNodeOrToken identifierToken)
        {
            SyntaxNode identifierNameNode = identifierToken.Parent;

            bool isPartOfMemberAccessChain = identifierNameNode.Parent is MemberAccessExpressionSyntax;

            if (!isPartOfMemberAccessChain)
                return false;

            bool isFirstMemberInChain = identifierNameNode.Parent.ChildNodesAndTokens().First() == identifierNameNode;

            return !isFirstMemberInChain;
        }
    }
}