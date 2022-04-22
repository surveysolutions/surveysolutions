using System;
using System.Collections.Generic;
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
        private static readonly string[] ForbiddenDateTimeStaticProperties = {"Now", "Today", "UtcNow"};
        public static readonly string ForbiddenDatetimeNow = "DateTime.Now";

        public IReadOnlyCollection<string> GetIdentifiersUsedInExpression(string expression)
        {
            string code = WrapToClass(expression);

            var tree = SyntaxFactory.ParseSyntaxTree(code);

            var tokens = tree
                .GetRoot()
                .ChildNodesAndTokens()
                .TreeToEnumerable(node =>
                    this.IsMemberAccessor(node) ? Enumerable.Empty<SyntaxNodeOrToken>() : node.ChildNodesAndTokens())
                .ToArray();

            var identifiers = tokens
                .Where(IsIdentifierToken)
                .Except(IsFunction)
                .Except(IsConstructorCall)
                .Except(IsLambdaParameter)
                .Select(token => token.ToString())
                .Except(string.IsNullOrEmpty);

            var forbiddenIdentifiers = tokens
                .Where(IsMemberAccessor)
                .Select(token => ForbiddenDatetimeNow);

            return identifiers
                .Union(forbiddenIdentifiers)
                .Distinct()
                .ToReadOnlyCollection();
        }

        private bool IsMemberAccessor(SyntaxNodeOrToken nodeOrToken)
        {
            if (!nodeOrToken.IsKind(SyntaxKind.SimpleMemberAccessExpression)) return false;
            var childNodesAndTokens = nodeOrToken.ChildNodesAndTokens().ToList();
            var right = childNodesAndTokens.Last();

            // Expression tree. Case 1:
            //     +--- . ---+
            //  DateTime    Now

            // Expression tree. Case 2:
            //         +--- . ---+
            //    +--- . ---+   Now
            // System    DateTime

            if (!right.IsKind(SyntaxKind.IdentifierName) ||
                !ForbiddenDateTimeStaticProperties.Contains(right.ToFullString().Trim()))
                return false;

            var left = childNodesAndTokens.First();
            var dateTimeNode = left.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                ? left.ChildNodesAndTokens().Last()
                : childNodesAndTokens.First();

            return dateTimeNode.ToFullString().Trim() == "DateTime";
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

        public bool ContainsBitwiseOr(string expression)
        {
            string code = WrapToClass(expression);

            var tree = SyntaxFactory.ParseSyntaxTree(code);

            return
                tree
                    .GetRoot()
                    .ChildNodesAndTokens()
                    .TreeToEnumerable(_ => _.ChildNodesAndTokens())
                    .Any(IsBitwiseOrExpression);
        }

        private static string WrapToClass(string expression) => $"class a {{ bool b() {{ return ({expression}); }} }} ";

        private static bool IsBitwiseAndExpression(SyntaxNodeOrToken nodeOrToken)
            => nodeOrToken.IsKind(SyntaxKind.BitwiseAndExpression);

        private static bool IsBitwiseOrExpression(SyntaxNodeOrToken nodeOrToken)
            => nodeOrToken.IsKind(SyntaxKind.BitwiseOrExpression);

        private static bool IsIdentifierToken(SyntaxNodeOrToken nodeOrToken)
            => nodeOrToken.IsToken
               && nodeOrToken.IsKind(SyntaxKind.IdentifierToken)
               && (nodeOrToken.Parent?.IsKind(SyntaxKind.IdentifierName) == true);

        private static bool IsFunction(SyntaxNodeOrToken identifierToken)
            => identifierToken.Parent?.Parent is InvocationExpressionSyntax;

        private static bool IsConstructorCall(SyntaxNodeOrToken identifierToken)
            => identifierToken.Parent?.Parent is ObjectCreationExpressionSyntax;

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
                .Where(nodeOrToken => nodeOrToken.IsNode && nodeOrToken.IsKind(SyntaxKind.Parameter))
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
                .Where(nodeOrToken => nodeOrToken.IsNode && nodeOrToken.IsKind(SyntaxKind.Parameter))
                .Select(parameterNode => parameterNode.ToString());

            return lambdaParameters.Contains(identifierToken.ToString());
        }

        private static TAncestor? FindAncestor<TAncestor>(SyntaxNodeOrToken nodeOrToken) where TAncestor: SyntaxNode
        {
            var referencedItem = nodeOrToken.Parent;
            while (referencedItem != null)
            {
                if (referencedItem is TAncestor node)
                    return node;
                
                referencedItem = referencedItem.Parent;
            }

            return null;
        }
    }
}
