using FormulaEvaluator;
using SpreadsheetUtilities;
using System.Dynamic;

namespace EvaluatorTester
{
    internal class Program
    {
        /// <summary>
        /// Main method to try and test Class1 aka evaluator
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {






            /*// simple arithmetic
            Console.WriteLine(Evaluator.Evaluate("1+1", null));
            Console.WriteLine(Evaluator.Evaluate("1-1", null));
            Console.WriteLine(Evaluator.Evaluate("2*2", null));
            Console.WriteLine(Evaluator.Evaluate("4/2", null));
            Console.WriteLine(Evaluator.Evaluate("1", null));

            // complex expression 
            Console.WriteLine(Evaluator.Evaluate("1+1*5*5*(4/3+9*75)/3", null));

            //unnecesary parenthesis
            Console.WriteLine(Evaluator.Evaluate("(11)", null));
            Console.WriteLine(Evaluator.Evaluate("(1)+(1)", null));

            // test with delegate for variables
            Console.WriteLine(Evaluator.Evaluate("A1+B2", MyLookup));
            Console.WriteLine(Evaluator.Evaluate("AA2 / 2", MyLookup));*/


        }

        /// <summary>
        /// Super dumb/basic lookup method to be passed into delegate for Evaluator to test how variables will behave
        /// </summary>
        /// <param name="str"></param> string to look up
        /// <returns></returns> return int value of the string
        private static int MyLookup(string str)
        {
            switch (str) 
            {
                case "A1":
                    return 1;
                case "B2":
                    return 2;
            }

            return 100;

        }
    }
}