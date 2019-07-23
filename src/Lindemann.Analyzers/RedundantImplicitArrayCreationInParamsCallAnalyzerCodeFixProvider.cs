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
using System;

namespace Lindemann.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RedundantImplicitArrayCreationInParamsCallAnalyzerCodeFixProvider)), Shared]
    public class RedundantImplicitArrayCreationInParamsCallAnalyzerCodeFixProvider : CodeFixProvider
    {
        private readonly string title = Resources.MD0002AnalyzerTitle;

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(RedundantImplicitArrayCreationInParamsCallAnalyzer.RedundantImplicitArrayCreationDiagnosticId); }
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
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => SimplifyParamsParametersAsync(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> SimplifyParamsParametersAsync(
            Document document,
            ExpressionSyntax newArray,
            CancellationToken cancellationToken)
        {
            var arg = (ArgumentSyntax)newArray.Parent;
            var argList = (ArgumentListSyntax)arg.Parent;

            var argListWithoutParamsArray = argList.Arguments.Remove(arg);

            if (!(newArray is ImplicitArrayCreationExpressionSyntax iaces))
            {
                throw new ArgumentException("invalid type of array creation expression.", nameof(newArray));
            }

            ArgumentListSyntax resultingArgs = SyntaxFactory.ArgumentList(
                argListWithoutParamsArray
                    .AddRange(iaces.Initializer.Expressions.Select(SyntaxFactory.Argument)));

            // Replace the old local declaration with the new local declaration.
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(argList, resultingArgs);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
