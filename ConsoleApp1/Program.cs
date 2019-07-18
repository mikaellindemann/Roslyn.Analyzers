using ClassLibrary1;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            object a = 1;
            Console.WriteLine("Hello World! %d", new[] { a });
            Class1.Do(new[] { a });
            Do(new[] { 1 });
            new AggregateException(new[] { new Exception(), new Exception() });
        }

        static void Do(params int[] args)
        {
            Console.WriteLine(args);
        }

    }
}
