using Lindemann.Analyzers.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Lindemann.Analyzers.Tests
{
    public class RedundantArrayCreationInParamsCallAnalyzerUnitTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RedundantArrayCreationInParamsCallAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new RedundantArrayCreationInParamsCallAnalyzerCodeFixProvider();
        }

        [Theory]
        [InlineData("")]
        [InlineData(IntParamsFixedArray)]
        [InlineData(RequiredParameterFollowedByParamsFixed)]
        public void WhenTestCodeIsValidNoDiagnosticIsTriggered(string testCode)
        {
            VerifyCSharpDiagnostic(testCode);
        }

        [Theory]
        [InlineData(NewInt32Array, NewInt32ArrayInputExpression, IntParamsFixedArray, 14, 16)]
        [InlineData(RequiredParameterFollowedByParams, RequiredParameterFollowedByParamsInputExpression, RequiredParameterFollowedByParamsFixed, 14, 19)]
        [InlineData(CallingExternalParamsMethodWithArray, CallingExternalParamsMethodWithArrayInputExpression, CallingExternalParamsMethodWithArrayFixed, 10, 42)]
        public void WhenDiagnosticIsRaisedFixUpdatesCode(
            string input,
            string inputExpression,
            string expectedOutput,
            int line,
            int column)
        {
            var expected = new DiagnosticResult
            {
                Id = RedundantArrayCreationInParamsCallAnalyzer.RedundantArrayCreationDiagnosticId,
                Message = string.Format(new LocalizableResourceString(nameof(Resources.MD0001AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)).ToString(), inputExpression),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                   new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                       }
            };

            VerifyCSharpDiagnostic(input, expected);

            VerifyCSharpFix(input, expectedOutput);
        }

        private const string NewInt32Array = @"
using System;

namespace ConsoleApp1
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            const int i = 1;
            const int j = 3;
            const int k = (2 * i) + j;

            Do(new int[] { i, j, k });
        }

        internal static void Do(params int[] xs)
        {
            Console.WriteLine(xs);
        }
    }
}
";

        private const string NewInt32ArrayInputExpression = @"new int[] { i, j, k }";

        private const string IntParamsFixedArray = @"
using System;

namespace ConsoleApp1
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            const int i = 1;
            const int j = 3;
            const int k = (2 * i) + j;

            Do(i, j, k);
        }

        internal static void Do(params int[] xs)
        {
            Console.WriteLine(xs);
        }
    }
}
";

        private const string RequiredParameterFollowedByParams = @"
using System;

namespace ConsoleApp1
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            const int i = 1;
            const int j = 3;
            const int k = (2 * i) + j;

            Do(i, new int[] { j, k });
        }

        internal static void Do(int required, params int[] xs)
        {
            Console.Write(required);
            Console.Write(' ');
            Console.WriteLine(xs);
        }
    }
}
";

        private const string RequiredParameterFollowedByParamsInputExpression = @"new int[] { j, k }";


        private const string RequiredParameterFollowedByParamsFixed = @"
using System;

namespace ConsoleApp1
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            const int i = 1;
            const int j = 3;
            const int k = (2 * i) + j;

            Do(i, j, k);
        }

        internal static void Do(int required, params int[] xs)
        {
            Console.Write(required);
            Console.Write(' ');
            Console.WriteLine(xs);
        }
    }
}
";

        private const string CallingExternalParamsMethodWithArray = @"
using System;

namespace ConsoleApp1
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            throw new AggregateException(new Exception[] { new Exception(), new Exception() });
        }
    }
}
";

        private const string CallingExternalParamsMethodWithArrayInputExpression = @"new Exception[] { new Exception(), new Exception() }";

        private const string CallingExternalParamsMethodWithArrayFixed = @"
using System;

namespace ConsoleApp1
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            throw new AggregateException(new Exception(), new Exception());
        }
    }
}
";

    }
}
