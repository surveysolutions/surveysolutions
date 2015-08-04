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
        public IEnumerable<string> GetIdentifiersUsedInExpression(string expression)
        {
            string code = string.Format("class a {{ bool b() {{ return ({0}); }} }} ", expression);

            return SyntaxFactory
                .ParseSyntaxTree(code).GetRoot().ChildNodesAndTokens().TreeToEnumerable(_ => _.ChildNodesAndTokens())
                .Where(IsIdentifierToken)
                .Except(IsFunction)
                .Except(IsConstructorCall)
                .Except(IsLambdaParameter)
                .Except(identifierToken => IsPropertyOrMethod(identifierToken) && !IsPropertyOfLambdaParameter(identifierToken))
                .Select(token => token.ToString())
                .Except(string.IsNullOrEmpty)
                .Distinct();
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
            return IsPartOfMemberAccessChain(identifierToken)
                && !IsFirstMemberInMemberAccessChain(identifierToken);
        }

        private static bool IsPropertyOfLambdaParameter(SyntaxNodeOrToken identifierToken)
        {
            return IsPartOfMemberAccessChain(identifierToken)
                && IsSecondMemberInMemberAccessChain(identifierToken)
                && DoesMemberAccessChainStartWithLambdaParameter(identifierToken.Parent.Parent);
        }

        private static bool IsFirstMemberInMemberAccessChain(SyntaxNodeOrToken identifierToken)
        {
            return identifierToken == GetFirstMemberAsIdentifierTokenFromMemberAccessChain(identifierToken.Parent.Parent);
        }

        private static bool IsSecondMemberInMemberAccessChain(SyntaxNodeOrToken identifierToken)
        {
            return identifierToken.Parent.Parent.ChildNodesAndTokens().Skip(2).First() == identifierToken.Parent;
        }

        private static bool DoesMemberAccessChainStartWithLambdaParameter(SyntaxNode memberAccessChainNode)
        {
            var firstMemberAsIdentifierToken = GetFirstMemberAsIdentifierTokenFromMemberAccessChain(memberAccessChainNode);

            return firstMemberAsIdentifierToken != null && IsLambdaParameter(firstMemberAsIdentifierToken);
        }

        private static SyntaxNodeOrToken GetFirstMemberAsIdentifierTokenFromMemberAccessChain(SyntaxNode memberAccessChainNode)
        {
            ChildSyntaxList childrenOfFirstMemberInChain = memberAccessChainNode.ChildNodesAndTokens().First().ChildNodesAndTokens();

            if (childrenOfFirstMemberInChain.Count != 1)
                return null;

            SyntaxNodeOrToken potentialIdentifierToken = childrenOfFirstMemberInChain.Single();

            return IsIdentifierToken(potentialIdentifierToken) ? potentialIdentifierToken : null;
        }

        private static bool IsPartOfMemberAccessChain(SyntaxNodeOrToken identifierToken)
        {
            return identifierToken.Parent.Parent is MemberAccessExpressionSyntax;
        }

        private static bool IsLambdaParameter(SyntaxNodeOrToken identifierToken)
        {
            return IsSimpleLambdaParameter(identifierToken) || IsParenthesizedLambdaParameter(identifierToken);
        }

        private static bool IsSimpleLambdaParameter(SyntaxNodeOrToken identifierToken)
        {
            var lambdaNode = FindAncestor<SimpleLambdaExpressionSyntax>(identifierToken);

            if (lambdaNode == null)
                return false;

            IEnumerable<string> lambdaParameters = lambdaNode
                .ChildNodesAndTokens()
                .Where(nodeOrToken => nodeOrToken.IsNode && nodeOrToken.CSharpKind() == SyntaxKind.Parameter)
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
                .Where(nodeOrToken => nodeOrToken.IsNode && nodeOrToken.CSharpKind() == SyntaxKind.Parameter)
                .Select(parameterNode => parameterNode.ToString());

            return lambdaParameters.Contains(identifierToken.ToString());
        }

        private static TAncestor FindAncestor<TAncestor>(SyntaxNodeOrToken nodeOrToken)
            where TAncestor : SyntaxNode
        {
            SyntaxNode ancestor = nodeOrToken.Parent;

            while (ancestor != null)
            {
                if (ancestor is TAncestor)
                    return (TAncestor) ancestor;

                ancestor = ancestor.Parent;
            }

            return null;
        }
    }
}