using Lindemann.Analyzers.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Lindemann.Analyzers.Tests
{
    public class TryGetValueDiscardContainsAnalyzerUnitTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TryGetValueDiscardAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TryGetValueDiscardContainsAnalyzerCodeFixProvider();
        }

        [Theory]
        [InlineData("")]
        [InlineData(HashSetViolationFixed)]
        public void WhenTestCodeIsValidNoDiagnosticIsTriggered(string testCode)
        {
            VerifyCSharpDiagnostic(testCode);
        }

        [Theory]
        [InlineData(HashSetViolation, HashSetViolationCurrentExpression, HashSetViolationContainsExpression, HashSetViolationFixed, 13, 17)]
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
                Id = TryGetValueDiscardAnalyzer.TryGetValueDiscardContainsDiagnosticId,
                Message = string.Format(new LocalizableResourceString(nameof(Resources.MD0010AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)).ToString(), inputExpression, containsExpression),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                }
            };

            VerifyCSharpDiagnostic(input, expected);

            VerifyCSharpFix(input, expectedOutput);
        }

        private const string HashSetViolation = @"
using System.Collections.Generic;

namespace Lindemann.Analyzers.Violations
{
    internal static class Program
    {
        private static readonly string arg = ""Hej"";
        private static void Main()
        {
            var set = new HashSet<string>();

            if (set.TryGetValue(arg, out var _))
            {

            }
        }
    }
}
";
        private const string HashSetViolationCurrentExpression = "set.TryGetValue(arg, out var _)";
        private const string HashSetViolationContainsExpression = "set.Contains(arg)";
        private const string HashSetViolationFixed = @"
using System.Collections.Generic;

namespace Lindemann.Analyzers.Violations
{
    internal static class Program
    {
        private static readonly string arg = ""Hej"";
        private static void Main()
        {
            var set = new HashSet<string>();

            if (set.Contains(arg))
            {

            }
        }
    }
}
";
    }
}
