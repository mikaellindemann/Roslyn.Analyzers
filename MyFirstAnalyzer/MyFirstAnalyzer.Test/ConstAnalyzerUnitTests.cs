using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace MyFirstAnalyzer.Test
{
    public class ConstAnalyzerUnitTests : CodeFixVerifier
    {

        [Theory]
        [InlineData(""),
         InlineData(VariableAssigned),
         InlineData(AlreadyConst),
         InlineData(NoInitializer),
         InlineData(InitializerNotConstant),
         InlineData(MultipleInitializers),
            InlineData(DeclarationIsInvalid),
            InlineData(ReferenceTypeIsntString)]
        public void WhenTestCodeIsValidNoDiagnosticIsTriggered(string testCode)
        {
            VerifyCSharpDiagnostic(testCode);
        }

        [Theory]
        [InlineData(LocalIntCouldBeConstant, LocalIntCouldBeConstantInputExpression, LocalIntCouldBeConstantFixed, 10, 13),
         InlineData(ConstantIsString, ConstantIsStringInputExpression, ConstantIsStringFixed, 10, 13),
         InlineData(DeclarationUsesVar, DeclarationUsesVarInputExpression, DeclarationUsesVarFixedHasType, 10, 13),
         InlineData(StringDeclarationUsesVar, StringDeclarationUsesVarInputExpression, StringDeclarationUsesVarFixedHasType, 10, 13)]
        public void WhenDiagnosticIsRaisedFixUpdatesCode(
            string test,
            string inputExpression,
            string fixTest,
            int line,
            int column)
        {
            var expected = new DiagnosticResult
            {
                Id = ConstAnalyzer.DiagnosticId,
                Message = string.Format(new LocalizableResourceString(nameof(Resources.MD0001AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)).ToString(), inputExpression),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            VerifyCSharpFix(test, fixTest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ConstAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ConstAnalyzer();
        }

        private const string VariableAssigned = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0;
            Console.WriteLine(i++);
        }
    }
}";

        private const string AlreadyConst = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            const int i = 0;
            Console.WriteLine(i);
        }
    }
}";

        private const string NoInitializer = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int i;
            i = 0;
            Console.WriteLine(i);
        }
    }
}";

        private const string InitializerNotConstant = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = DateTime.Now.DayOfYear;
            Console.WriteLine(i);
        }
    }
}";

        private const string MultipleInitializers = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0, j = DateTime.Now.DayOfYear;
            Console.WriteLine(i, j);
        }
    }
}";

        private const string DeclarationIsInvalid = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int x = ""abc"";
        }
    }
}";

        private const string ReferenceTypeIsntString = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            object s = ""abc"";
        }
    }
}";

        private const string LocalIntCouldBeConstant = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0;
            Console.WriteLine(i);
        }
    }
}";

        private const string LocalIntCouldBeConstantInputExpression = @"int i = 0;";

        private const string LocalIntCouldBeConstantFixed = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            const int i = 0;
            Console.WriteLine(i);
        }
    }
}";

        private const string ConstantIsString = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = ""abc"";
        }
    }
}";

        private const string ConstantIsStringInputExpression = @"string s = ""abc"";";

        private const string ConstantIsStringFixed = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            const string s = ""abc"";
        }
    }
}";

        private const string DeclarationUsesVar = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var item = 4;
        }
    }
}";

        private const string DeclarationUsesVarInputExpression = @"var item = 4;";

        private const string DeclarationUsesVarFixedHasType = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            const int item = 4;
        }
    }
}";
        private const string StringDeclarationUsesVar = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var item = ""abc"";
        }
    }
}";

        private const string StringDeclarationUsesVarInputExpression = @"var item = ""abc"";";

        private const string StringDeclarationUsesVarFixedHasType = @"
using System;

namespace MakeConstTest
{
    class Program
    {
        static void Main(string[] args)
        {
            const string item = ""abc"";
        }
    }
}";

    }
}
