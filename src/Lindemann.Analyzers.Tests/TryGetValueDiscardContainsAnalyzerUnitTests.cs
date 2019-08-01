﻿using Lindemann.Analyzers.Tests.Helpers;
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
        [InlineData(DiscardTupleAssignment)]
        [InlineData(NotDiscarding)]
        [InlineData(NotTryGetValue)]
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

        private const string DiscardTupleAssignment = @"
namespace Lindemann.Analyzers.Violations
{
    internal static class Program
    {
        private static void Main()
        {
            var (a, _) = Stuffs();

            (var b, var _) = Stuffs();

            System.Console.WriteLine(a);
            System.Console.WriteLine(b);
        }

        private static (int a, int b) Stuffs()
        {
            return (4, 3);
        }
    }
}
";

        private const string NotDiscarding = @"
namespace Lindemann.Analyzers.Violations
{
    internal static class Program
    {
        private static void Main()
        {
            int b = 5;

            if (TryStuffs(b, out b))
            {
                System.Console.WriteLine(b);
            }
        }

        private static bool TryStuffs(int a, out int b)
        {
            b = a;
            return a != 0;
        }
    }
}
";

        private const string NotTryGetValue = @"
namespace Lindemann.Analyzers.Violations
{
    internal static class Program
    {
        private static void Main()
        {
            const int b = 5;

            if (Program.TryStuffs(b, out var _))
            {
                System.Console.WriteLine(b);
            }
        }

        private static bool TryStuffs(int a, out int b)
        {
            b = a;
            return a != 0;
        }
    }
}";
    }
}
