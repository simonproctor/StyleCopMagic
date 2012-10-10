﻿// -----------------------------------------------------------------------
// <copyright file="SA1200.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace StyleCopMagic.OrderingRules
{
    using System.Linq;
    using Roslyn.Compilers.CSharp;

    public class SA1200 : RuleRewriter
    {
        public override SyntaxNode Visit(SyntaxNode node)
        {
            // TODO: For some reason a null can get passed in here. Check again with newer CTP than June 2012.
            if (node != null && node.Kind == SyntaxKind.CompilationUnit)
            {
                UsingDirectiveSyntax[] usingNodes = node.ChildNodes().Where(x => x.Kind == SyntaxKind.UsingDirective).Cast<UsingDirectiveSyntax>().ToArray();
                NamespaceDeclarationSyntax namespaceNode = (NamespaceDeclarationSyntax)node.ChildNodes().FirstOrDefault(x => x.Kind == SyntaxKind.NamespaceDeclaration);

                if (usingNodes.Length > 0 && namespaceNode != null)
                {
                    // Remove the existing using statements.
                    node = node.ReplaceNodes(usingNodes, (a, b) => null);

                    // Remove the leading trivia from the first using directive. This will be added to the namespace.
                    SyntaxTriviaList leadingTrivia = usingNodes[0].GetLeadingTrivia();
                    usingNodes[0] = usingNodes[0].WithLeadingTrivia();

                    // Add a trailing newline to the last using.
                    usingNodes[usingNodes.Length - 1] = usingNodes.Last().WithTrailingTrivia(
                        Syntax.CarriageReturnLineFeed,
                        Syntax.CarriageReturnLineFeed);

                    // Create a new namespace statment with the usings and the leading trivia we removed earlier.
                    SyntaxNode newNamespaceNode = namespaceNode
                        .WithUsings(Syntax.List<UsingDirectiveSyntax>(usingNodes))
                        .WithLeadingTrivia(leadingTrivia);

                    // Replace the namespace with the one with usings.
                    node = node.ReplaceNodes(
                        node.ChildNodes().Take(1),
                        (a, b) => newNamespaceNode);

                    return node;
                }
            }

            return base.Visit(node);
        }

    }
}