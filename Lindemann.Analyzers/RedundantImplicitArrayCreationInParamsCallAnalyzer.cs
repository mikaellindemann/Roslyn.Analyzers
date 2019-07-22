using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Lindemann.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantImplicitArrayCreationInParamsCallAnalyzer : ParamsParametersAnalyzerBase
    {
        public const string RedundantImplicitArrayCreationDiagnosticId = "MD0002";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MD0002AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.MD0002AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MD0002AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor RedundantImplicitArrayCreationRule = new DiagnosticDescriptor(
            RedundantImplicitArrayCreationDiagnosticId, 
            Title, 
            MessageFormat, 
            Category, 
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(RedundantImplicitArrayCreationRule);

        public override void Initialize(AnalysisContext context)
        {

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeImplicitArrayCreationExpressionNode, SyntaxKind.ImplicitArrayCreationExpression);
        }

        private void AnalyzeImplicitArrayCreationExpressionNode(SyntaxNodeAnalysisContext context)
        {
            var es = (ImplicitArrayCreationExpressionSyntax)context.Node;

            if (!(IsCallingParamsMethod(context.SemanticModel, es, context.CancellationToken)
                || IsCallingParamsConstructor(context.SemanticModel, es, context.CancellationToken)))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(RedundantImplicitArrayCreationRule, es.GetLocation(), es));
        }
    }
}