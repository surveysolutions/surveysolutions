using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class RoslynExpressionProcessor : IExpressionProcessor
    {
        public IReadOnlyCollection<string> GetIdentifiersUsedInExpression(string expression)
        {
            string code = WrapToClass(expression);

            var tree = SyntaxFactory.ParseSyntaxTree(code);

            return tree
                .GetRoot()
                .ChildNodesAndTokens()
                .TreeToEnumerable(_ => _.ChildNodesAndTokens())
                .Where(IsIdentifierToken)
                .Except(IsFunction)
                .Except(IsConstructorCall)
                .Except(IsLambdaParameter)
                .Select(token => token.ToString())
                .Except(string.IsNullOrEmpty)
                .Distinct()
                .ToReadOnlyCollection();
        }

        public bool ContainsBitwiseAnd(string expression)
        {
            string code = WrapToClass(expression);

            var tree = SyntaxFactory.ParseSyntaxTree(code);

            return
                tree
                .GetRoot()
                .ChildNodesAndTokens()
                .TreeToEnumerable(_ => _.ChildNodesAndTokens())
                .Any(IsBitwiseAndExpression);
        }

        private static string WrapToClass(string expression) => $"class a {{ bool b() {{ return ({expression}); }} }} ";

        private static bool IsBitwiseAndExpression(SyntaxNodeOrToken nodeOrToken)
            => nodeOrToken.Kind() == SyntaxKind.BitwiseAndExpression;

        private static bool IsIdentifierToken(SyntaxNodeOrToken nodeOrToken)
            => nodeOrToken.IsToken
            && nodeOrToken.Kind() == SyntaxKind.IdentifierToken
            && nodeOrToken.Parent.Kind() == SyntaxKind.IdentifierName;

        private static bool IsFunction(SyntaxNodeOrToken identifierToken)
            => identifierToken.Parent.Parent is InvocationExpressionSyntax;

        private static bool IsConstructorCall(SyntaxNodeOrToken identifierToken)
            => identifierToken.Parent.Parent is ObjectCreationExpressionSyntax;

        private static bool IsLambdaParameter(SyntaxNodeOrToken identifierToken)
            => IsSimpleLambdaParameter(identifierToken)
            || IsParenthesizedLambdaParameter(identifierToken);

        private static bool IsSimpleLambdaParameter(SyntaxNodeOrToken identifierToken)
        {
            var lambdaNode = FindAncestor<SimpleLambdaExpressionSyntax>(identifierToken);

            if (lambdaNode == null)
                return false;

            IEnumerable<string> lambdaParameters = lambdaNode
                .ChildNodesAndTokens()
                .Where(nodeOrToken => nodeOrToken.IsNode && nodeOrToken.Kind() == SyntaxKind.Parameter)
                .Select(parameterNode => parameterNode.ToString());

            return lambdaParameters.Contains(identifierToken.ToString());
        }

        private static bool IsParenthesizedLambdaParameter(SyntaxNodeOrToken identifierToken)
        {
            var lambdaNode = FindAncestor<ParenthesizedLambdaExpressionSyntax>(identifierToken);

            if (lambdaNode == null)
                return false;

            IEnumerable<string> lambdaParameters = lambdaNode
                .ChildNodesAndTokens()
                .First()
                .ChildNodesAndTokens()
                .Where(nodeOrToken => nodeOrToken.IsNode && nodeOrToken.Kind() == SyntaxKind.Parameter)
                .Select(parameterNode => parameterNode.ToString());

            return lambdaParameters.Contains(identifierToken.ToString());
        }

        private static TAncestor FindAncestor<TAncestor>(SyntaxNodeOrToken nodeOrToken)
            where TAncestor : SyntaxNode
            => (TAncestor) nodeOrToken.Parent.UnwrapReferences(ancestor => ancestor.Parent).FirstOrDefault(ancestor => ancestor is TAncestor);
    }
}