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

namespace MyFirstAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExplicitArrayToParamsParametersAnalyzerCodeFixProvider)), Shared]
    public class ExplicitArrayToParamsParametersAnalyzerCodeFixProvider : CodeFixProvider
    {
        private readonly string title = Resources.MD1001AnalyzerTitle;

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ExplicitArrayToParamsParametersAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
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

            ArgumentListSyntax resultingArgs;
            if (newArray is ImplicitArrayCreationExpressionSyntax iaces)
            {
                resultingArgs = SyntaxFactory.ArgumentList(
                    argListWithoutParamsArray
                        .AddRange(iaces.Initializer.Expressions.Select(SyntaxFactory.Argument)));
            }
            else if (newArray is ArrayCreationExpressionSyntax aces)
            {
                resultingArgs = SyntaxFactory.ArgumentList(
                    argListWithoutParamsArray
                        .AddRange(aces.Initializer.Expressions.Select(SyntaxFactory.Argument)));
            }
            else
            {
                throw new ArgumentException("invalid type of array creation expression.", nameof(newArray));
            }

            // Replace the old local declaration with the new local declaration.
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(argList, resultingArgs);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
