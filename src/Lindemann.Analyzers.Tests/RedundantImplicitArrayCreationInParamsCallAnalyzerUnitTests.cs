using Lindemann.Analyzers.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Lindemann.Analyzers.Tests
{
    public class RedundantImplicitArrayCreationInParamsCallAnalyzerUnitTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RedundantImplicitArrayCreationInParamsCallAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new RedundantImplicitArrayCreationInParamsCallAnalyzerCodeFixProvider();
        }

        [Theory]
        [InlineData("")]
        [InlineData(LocalVariableNotTargeted)]
        [InlineData(IntParamsFixedArray)]
        [InlineData(RequiredParameterFollowedByParamsFixed)]
        [InlineData(ParamsArrayTypeNotMatchingShouldHaveNoWarning)]
        [InlineData(ParamsArrayTypeNotMatchingConstructorShouldHaveNoWarning)]
        public void WhenTestCodeIsValidNoDiagnosticIsTriggered(string testCode)
        {
            VerifyCSharpDiagnostic(testCode);
        }

        [Theory]
        [InlineData(NewImplicitlyTypedInt32Array, NewImplicitlyTypedInt32ArrayInputExpression, IntParamsFixedArray, 14, 16)]
        [InlineData(RequiredParameterFollowedByParams, RequiredParameterFollowedByParamsInputExpression, RequiredParameterFollowedByParamsFixed, 14, 19)]
        [InlineData(CallingExternalParamsMethodWithImplicitTypedArray, CallingExternalParamsMethodWithImplicitTypedArrayInputExpression, CallingExternalParamsMethodWithImplicitTypedArrayFixed, 10, 42)]
        public void WhenDiagnosticIsRaisedFixUpdatesCode(
            string input,
            string inputExpression,
            string expectedOutput,
            int line,
            int column)
        {
            var expected = new DiagnosticResult
            {
                Id = RedundantImplicitArrayCreationInParamsCallAnalyzer.RedundantImplicitArrayCreationDiagnosticId,
                Message = string.Format(new LocalizableResourceString(nameof(Resources.MD0002AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)).ToString(), inputExpression),
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

        private const string CallingExternalParamsMethodWithImplicitTypedArray = @"
using System;

namespace ConsoleApp1
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            throw new AggregateException(new[] { new Exception(), new Exception() });
        }
    }
}
";

        private const string CallingExternalParamsMethodWithImplicitTypedArrayInputExpression = @"new[] { new Exception(), new Exception() }";

        private const string CallingExternalParamsMethodWithImplicitTypedArrayFixed = @"
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

        private const string ParamsArrayTypeNotMatchingShouldHaveNoWarning = @"
using System;

namespace ConsoleApp1
{
    internal static class Program
    {
        internal static void Main()
        {
            Console.WriteLine(""Hello, {0}!"", new[] { ""world"" });
        }
    }
}";

        private const string ParamsArrayTypeNotMatchingConstructorShouldHaveNoWarning = @"
namespace ConsoleApp1
{
    internal class Program
    {
        public Program(params object[] ignore)
        {

        }

        internal static void Main()
        {
            new Program(new[] { ""world"" });
        }
    }
}";
    }
}
