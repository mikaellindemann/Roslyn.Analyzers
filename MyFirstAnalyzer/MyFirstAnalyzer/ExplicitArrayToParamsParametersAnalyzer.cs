using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace MyFirstAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExplicitArrayToParamsParametersAnalyzer : ParamsParametersAnalyzerBase
    {
        public const string DiagnosticId = "MD1001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MD1001AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.MD1001AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MD1001AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeImplicitArrayCreationExpressionNode, SyntaxKind.ImplicitArrayCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeArrayCreationExpressionNode, SyntaxKind.ArrayCreationExpression);
        }

        private void AnalyzeImplicitArrayCreationExpressionNode(SyntaxNodeAnalysisContext context)
        {
            var es = (ImplicitArrayCreationExpressionSyntax)context.Node;

            if (!IsCallingParamsMethod(context.SemanticModel, es, context.CancellationToken, out var calledMethod, out var als))
            {
                return;
            }

            if (WouldCallOverload(context.SemanticModel, calledMethod, als, es.Initializer, context.CancellationToken))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, es.GetLocation(), es));
        }

        private void AnalyzeArrayCreationExpressionNode(SyntaxNodeAnalysisContext context)
        {
            var es = (ArrayCreationExpressionSyntax)context.Node;

            if (!IsCallingParamsMethod(context.SemanticModel, es, context.CancellationToken, out var calledMethod, out var als))
            {
                return;
            }

            if (WouldCallOverload(context.SemanticModel, calledMethod, als, es.Initializer, context.CancellationToken))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, es.GetLocation(), es));
        }
    }
}