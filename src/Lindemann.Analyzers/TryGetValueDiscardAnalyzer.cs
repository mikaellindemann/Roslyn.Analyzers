using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Lindemann.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TryGetValueDiscardAnalyzer : DiagnosticAnalyzer
    {
        public const string TryGetValueDiscardContainsDiagnosticId = "MD0010";
        public const string TryGetValueDiscardContainsKeyDiagnosticId = "MD0011";

        private const string Category = "Performance";

        private static readonly DiagnosticDescriptor TryGetValueDiscardContainsRule = new DiagnosticDescriptor(
            TryGetValueDiscardContainsDiagnosticId,
            new LocalizableResourceString(nameof(Resources.MD0010AnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MD0010AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category, 
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description: new LocalizableResourceString(nameof(Resources.MD0010AnalyzerDescription), Resources.ResourceManager, typeof(Resources)));

        private static readonly DiagnosticDescriptor TryGetValueDiscardContainsKeyRule = new DiagnosticDescriptor(
            TryGetValueDiscardContainsKeyDiagnosticId,
            new LocalizableResourceString(nameof(Resources.MD0011AnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MD0011AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.MD0011AnalyzerDescription), Resources.ResourceManager, typeof(Resources)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(TryGetValueDiscardContainsRule, TryGetValueDiscardContainsKeyRule);

        public override void Initialize(AnalysisContext context)
        {

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeDiscardDesignationSyntax, SyntaxKind.DiscardDesignation);
        }

        private void AnalyzeDiscardDesignationSyntax(SyntaxNodeAnalysisContext context)
        {
            var es = (DiscardDesignationSyntax)context.Node;

            if (!(es.Parent is DeclarationExpressionSyntax de))
            {
                return;
            }

            if (!(de.Parent is ArgumentSyntax argS))
            {
                return;
            }

            if (!argS.ChildTokens().FirstOrDefault().IsKind(SyntaxKind.OutKeyword))
            {
                return;
            }

            if (!(argS.Parent is ArgumentListSyntax als))
            {
                return;
            }

            if (!(als.Parent is InvocationExpressionSyntax ies))
            {
                return;
            }

            if (!(ies.ChildNodes().FirstOrDefault() is MemberAccessExpressionSyntax maes) || !maes.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                return;
            }

            if (!(context.SemanticModel.GetSymbolInfo(maes, context.CancellationToken).Symbol is IMethodSymbol ims))
            {
                return;
            }

            if (!ims.Name.Equals("TryGetValue", StringComparison.InvariantCulture))
            {
                return;
            }

            if (TryGetMethod(context.SemanticModel, ims.ContainingType, "Contains", als, context.CancellationToken, out var symbol))
            {
                var replacement = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            maes.Expression,
                            SyntaxFactory.IdentifierName(symbol.Name)),
                        als.WithArguments(
                            als.Arguments.RemoveAt(als.Arguments.Count - 1)));
                context.ReportDiagnostic(Diagnostic.Create(TryGetValueDiscardContainsRule, ies.GetLocation(), ies, replacement));
            }
            else if (TryGetMethod(context.SemanticModel, ims.ContainingType, "ContainsKey", als, context.CancellationToken, out symbol))
            {
                var replacement = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            maes.Expression,
                            SyntaxFactory.IdentifierName(symbol.Name)),
                        als.WithArguments(
                            als.Arguments.RemoveAt(als.Arguments.Count - 1)));
                context.ReportDiagnostic(Diagnostic.Create(TryGetValueDiscardContainsKeyRule, ies.GetLocation(), ies, replacement));
            }
        }

        private bool TryGetMethod(SemanticModel semanticModel, INamedTypeSymbol containingType, string methodName, ArgumentListSyntax als, CancellationToken ct, out IMethodSymbol methodSymbol)
        {
            methodSymbol = null;

            foreach (var method in containingType.GetMembers(methodName).OfType<IMethodSymbol>())
            {
                if (method.Parameters.Length != als.Arguments.Count - 1)
                {
                    continue;
                }

                var matches = true;
                for (int i = 0; i < method.Parameters.Length; i++)
                {

                    if (!Equals(
                            method.Parameters[i].Type,
                            semanticModel.GetTypeInfo(als.Arguments[i].Expression, ct).Type))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    methodSymbol = method;
                    return true;
                }
            }

            return false;
        }
    }
}