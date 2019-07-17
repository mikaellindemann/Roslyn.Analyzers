using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace MyFirstAnalyzer.Test
{
    public class ExplicitArrayToParamsParametersAnalyzerUnitTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExplicitArrayToParamsParametersAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ExplicitArrayToParamsParametersAnalyzerCodeFixProvider();
        }

        [Theory]
        [InlineData("")]
        [InlineData(LocalVariableNotTargeted)]
        [InlineData(IntParamsFixedArray)]
        [InlineData(RequiredParameterFollowedByParamsFixed)]
        [InlineData(RefactorWouldCallOtherMethod)]
        [InlineData(RefactorWouldCallOtherMethod2)]
        public void WhenTestCodeIsValidNoDiagnosticIsTriggered(string testCode)
        {
            VerifyCSharpDiagnostic(testCode);
        }

        [Theory]
        [InlineData(NewImplicitlyTypedInt32Array, NewImplicitlyTypedInt32ArrayInputExpression, IntParamsFixedArray, 14, 16)]
        [InlineData(NewInt32Array, NewInt32ArrayInputExpression, IntParamsFixedArray, 14, 16)]
        [InlineData(RequiredParameterFollowedByParams, RequiredParameterFollowedByParamsInputExpression, RequiredParameterFollowedByParamsFixed, 14, 19)]
        [InlineData(CallingExternalParamsMethodWithImplicitTypedArray, CallingExternalParamsMethodWithImplicitTypedArrayInputExpression, CallingExternalParamsMethodWithImplicitTypedArrayFixed, 14, 19)]
        public void WhenDiagnosticIsRaisedFixUpdatesCode(
            string input,
            string inputExpression,
            string expectedOutput,
            int line,
            int column)
        {
            var expected = new DiagnosticResult
            {
                Id = ExplicitArrayToParamsParametersAnalyzer.DiagnosticId,
                Message = string.Format(new LocalizableResourceString(nameof(Resources.MD1001AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)).ToString(), inputExpression),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                   new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                       }
            };

            VerifyCSharpDiagnostic(input, expected);

            VerifyCSharpFix(input, expectedOutput);
        }

        private const string LocalVariableNotTargeted = @"
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

            var arguments = new[] { i, j, k };

            Do(arguments);
        }

        internal static void Do<T>(params T[] xs)
        {
            Console.WriteLine(xs);
        }
    }
}
";

        private const string NewImplicitlyTypedInt32Array = @"
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

            Do(new[] { i, j, k });
        }

        internal static void Do(params int[] xs)
        {
            Console.WriteLine(xs);
        }
    }
}
";

        private const string NewImplicitlyTypedInt32ArrayInputExpression = @"new[] { i, j, k }";

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

            Do(i, new[] { j, k });
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

        private const string RequiredParameterFollowedByParamsInputExpression = @"new[] { j, k }";


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

        private const string RefactorWouldCallOtherMethod = @"
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

            Do(new[] { i, j, k });
        }

        internal static void Do(params int[] xs)
        {
            Console.WriteLine(xs);
        }

        internal static void Do(int a, int b, int c)
        {
            Console.WriteLine(a + b + c);
        }
    }
}
";

        private const string RefactorWouldCallOtherMethod2 = @"
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

            Do(new[] { i, j, k });
        }

        internal static void Do(params int[] xs)
        {
            Console.WriteLine(xs);
        }

        internal static void Do(int a, params int[] xs)
        {
            Console.WriteLine(a + b + c);
        }
    }
}
";

        private const string CallingExternalParamsMethodWithImplicitTypedArray = @"
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

            Console.WriteLine(""%d %d %d"", new[] { (object)i, (object)j, (object)k });
        }
    }
}
";

        private const string CallingExternalParamsMethodWithImplicitTypedArrayInputExpression = @"new[] { (object)i, (object)j, (object)k }";

        private const string CallingExternalParamsMethodWithImplicitTypedArrayFixed = @"
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

            Console.WriteLine(""%d %d %d"", (object)i, (object)j, (object)k);
        }
    }
}
";

    }
}
