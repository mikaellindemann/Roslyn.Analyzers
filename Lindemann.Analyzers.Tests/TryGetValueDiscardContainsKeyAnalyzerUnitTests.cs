using Lindemann.Analyzers.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Lindemann.Analyzers.Tests
{
    public class TryGetValueDiscardContainsKeyAnalyzerUnitTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TryGetValueDiscardAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TryGetValueDiscardContainsKeyAnalyzerCodeFixProvider();
        }

        [Theory]
        [InlineData("")]
        [InlineData(DictionaryViolationFixed)]
        public void WhenTestCodeIsValidNoDiagnosticIsTriggered(string testCode)
        {
            VerifyCSharpDiagnostic(testCode);
        }

        [Theory]
        [InlineData(DictionaryViolation, DictionaryViolationCurrentExpression, DictionaryViolationContainsExpression, DictionaryViolationFixed, 13, 17)]
        public void WhenDiagnosticIsRaisedFixUpdatesCode(
            string input,
            string inputExpression,
            string containsExpression,
            string expectedOutput,
            int line,
            int column)
        {
            var expected = new DiagnosticResult
            {
                Id = TryGetValueDiscardAnalyzer.TryGetValueDiscardContainsKeyDiagnosticId,
                Message = string.Format(new LocalizableResourceString(nameof(Resources.MD0011AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)).ToString(), inputExpression, containsExpression),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                }
            };

            VerifyCSharpDiagnostic(input, expected);

            VerifyCSharpFix(input, expectedOutput);
        }

        private const string DictionaryViolation = @"
using System.Collections.Generic;

namespace Lindemann.Analyzers.Violations
{
    internal static class Program
    {
        private static readonly string arg = ""Hej"";
        private static void Main()
        {
            var dict = new Dictionary<string, string>();

            if (dict.TryGetValue(arg, out var _))
            {

            }
        }
    }
}
";
        private const string DictionaryViolationCurrentExpression = "dict.TryGetValue(arg, out var _)";
        private const string DictionaryViolationContainsExpression = "dict.ContainsKey(arg)";
        private const string DictionaryViolationFixed = @"
using System.Collections.Generic;

namespace Lindemann.Analyzers.Violations
{
    internal static class Program
    {
        private static readonly string arg = ""Hej"";
        private static void Main()
        {
            var dict = new Dictionary<string, string>();

            if (dict.ContainsKey(arg))
            {

            }
        }
    }
}
";
    }
}
