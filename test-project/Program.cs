using System;

// Bilerek hatalı kod — BugBuddy testi için
namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            // CS1002: noktalı virgül eksik
            int x = 5

            // CS0103: tanımsız değişken
            Console.WriteLine(unknownVariable);

            // CS0246: bilinmeyen tip
            ILogger logger = null;
        }
    }
}
