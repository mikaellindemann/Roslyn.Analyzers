using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Lindemann.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantArrayCreationInParamsCallAnalyzer : ParamsParametersAnalyzerBase
    {
        public const string RedundantArrayCreationDiagnosticId = "MD0001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MD0001AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.MD0001AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MD0001AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor RedundantArrayCreationRule = new DiagnosticDescriptor(
            RedundantArrayCreationDiagnosticId, 
            Title, 
            MessageFormat, 
            Category, 
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(RedundantArrayCreationRule);

        public override void Initialize(AnalysisContext context)
        {

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeArrayCreationExpressionNode, SyntaxKind.ArrayCreationExpression);
        }

        private void AnalyzeArrayCreationExpressionNode(SyntaxNodeAnalysisContext context)
        {
            var es = (ArrayCreationExpressionSyntax)context.Node;

            if (!(IsCallingParamsMethod(context.SemanticModel, es, context.CancellationToken)
                || IsCallingParamsConstructor(context.SemanticModel, es, context.CancellationToken)))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(RedundantArrayCreationRule, es.GetLocation(), es));
        }
    }
}