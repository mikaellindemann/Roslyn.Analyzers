using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lindemann.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TryGetValueDiscardContainsAnalyzerCodeFixProvider)), Shared]
    public class TryGetValueDiscardContainsAnalyzerCodeFixProvider : CodeFixProvider
    {
        private readonly string title = Resources.MD0010AnalyzerTitle;

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(TryGetValueDiscardAnalyzer.TryGetValueDiscardContainsDiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var invocation = root.FindToken(diagnosticSpan.Start).Parent.Ancestors().OfType<InvocationExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReplaceTryGetValueWithContainsAsync(context.Document, root, invocation, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private Task<Document> ReplaceTryGetValueWithContainsAsync(
            Document document,
            SyntaxNode root,
            InvocationExpressionSyntax invocation,
            CancellationToken cancellationToken)
        {
            var maes = (MemberAccessExpressionSyntax)invocation.Expression;
            var als = invocation.ArgumentList;

            var replacement = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            maes.Expression,
                            SyntaxFactory.IdentifierName("Contains")),
                        als.WithArguments(
                            als.Arguments.RemoveAt(als.Arguments.Count - 1)));

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(invocation, replacement)));

        }
    }
}
