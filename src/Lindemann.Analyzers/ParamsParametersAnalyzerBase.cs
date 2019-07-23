using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading;

namespace Lindemann.Analyzers
{
    public abstract class ParamsParametersAnalyzerBase : DiagnosticAnalyzer
    {
        protected bool IsCallingParamsConstructor(SemanticModel semanticModel, SyntaxNode sn, CancellationToken ct)
        {
            if (!(sn.Parent is ArgumentSyntax arg))
            {
                return false;
            }

            if (!(arg.Parent is ArgumentListSyntax als))
            {
                return false;
            }

            if (!(als.Parent is ObjectCreationExpressionSyntax oces))
            {
                return false;
            }

            if (!(semanticModel.GetSymbolInfo(oces, ct).Symbol is IMethodSymbol calledConstructor))
            {
                return false;
            }

            var paramIndex = als.Arguments.IndexOf(arg);

            if (calledConstructor.Parameters.Length <= paramIndex)
            {
                return false;
            }

            if (!calledConstructor.Parameters[paramIndex].IsParams)
            {
                return false;
            }

            return true;
        }

        protected bool IsCallingParamsMethod(SemanticModel semanticModel, SyntaxNode sn, CancellationToken ct)
        {
            if (!(sn.Parent is ArgumentSyntax arg))
            {
                return false;
            }

            if (!(arg.Parent is ArgumentListSyntax als))
            {
                return false;
            }

            if (!(als.Parent is InvocationExpressionSyntax ies))
            {
                return false;
            }

            if (!(semanticModel.GetSymbolInfo(ies, ct).Symbol is IMethodSymbol calledMethod))
            {
                return false;
            }

            var paramIndex = als.Arguments.IndexOf(arg);

            if (calledMethod.Parameters.Length <= paramIndex)
            {
                return false;
            }

            if (!calledMethod.Parameters[paramIndex].IsParams)
            {
                return false;
            }

            return true;
        }
    }
}
