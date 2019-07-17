using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using System.Linq;
using System.Threading;

namespace MyFirstAnalyzer
{
    public abstract class ParamsParametersAnalyzerBase : DiagnosticAnalyzer
    {
        protected bool IsCallingParamsMethod(SemanticModel semanticModel, SyntaxNode sn, CancellationToken ct, out IMethodSymbol calledMethod, out ArgumentListSyntax als)
        {
            calledMethod = null;
            als = null;
            if (!(sn.Parent is ArgumentSyntax arg))
            {
                return false;
            }

            als = arg.Parent as ArgumentListSyntax;
            if (als == null)
            {
                return false;
            }

            if (!(als.Parent is InvocationExpressionSyntax ies))
            {
                return false;
            }

            calledMethod = semanticModel.GetSymbolInfo(ies, ct).Symbol as IMethodSymbol;
            if (calledMethod == null)
            {
                // If this fails, it is possibly because the called method is not part of the source code.
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

        protected bool WouldCallOverload(SemanticModel semanticModel, IMethodSymbol calledMethod, ArgumentListSyntax als, InitializerExpressionSyntax ies, CancellationToken ct)
        {
            // Verify that making the change will not accidentally call another overload of the method.
            if (calledMethod.CanBeReferencedByName)
            {
                var totalParameterCount = calledMethod.Parameters.Length - 1 + ies.Expressions.Count;

                var argumentExpressionTypes = als.Arguments
                    .Select(x => semanticModel.GetTypeInfo(x.Expression, ct).Type)
                    .ToList();
                argumentExpressionTypes.RemoveAt(argumentExpressionTypes.Count - 1);

                var initializerExpressionTypes = ies.Expressions
                    .Select(x => semanticModel.GetTypeInfo(x, ct).Type)
                    .ToArray();

                foreach (var member in calledMethod.ContainingType.GetMembers(calledMethod.Name).OfType<IMethodSymbol>())
                {
                    if (Equals(member, calledMethod))
                    {
                        continue;
                    }

                    var requiredParameters = member.Parameters.Count(x => !x.IsParams);

                    if (requiredParameters > totalParameterCount)
                    {
                        continue;
                    }

                    var isMemberParams = member.Parameters.Any(x => x.IsParams);

                    // TODO: Fix below code to include non-params parameters in the method call.
                    var matchesOverload = true;
                    for (int i = 0; i < requiredParameters; i++)
                    {
                        if (i < argumentExpressionTypes.Count)
                        {
                            if (!Equals(member.Parameters[i].Type, argumentExpressionTypes[i]))
                            {
                                matchesOverload = false;
                                break;
                            }
                        }
                        else
                        {
                            if (!Equals(member.Parameters[i].Type, initializerExpressionTypes[i - argumentExpressionTypes.Count]))
                            {
                                matchesOverload = false;
                                break;
                            }
                        }
                    }

                    if (isMemberParams)
                    {
                        var paramsParameterType = (IArrayTypeSymbol)member.Parameters.Where(x => x.IsParams).Single().Type;
                        var elementType = paramsParameterType.ElementType;
                        for (int i = requiredParameters; i < totalParameterCount; i++)
                        {
                            if (i < argumentExpressionTypes.Count)
                            {
                                if (!Equals(elementType, argumentExpressionTypes[i]))
                                {
                                    matchesOverload = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (!Equals(elementType, initializerExpressionTypes[i - argumentExpressionTypes.Count]))
                                {
                                    matchesOverload = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (matchesOverload)
                    {
                        // Code-fix would result in calling other method.
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
