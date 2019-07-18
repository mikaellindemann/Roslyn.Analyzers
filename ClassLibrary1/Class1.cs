using System;

namespace ClassLibrary1
{
    public static class Class1
    {
        public static void Do(params object[] args)
        {
            Console.WriteLine(args);
        }
    }
}
