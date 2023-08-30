using FormulaEvaluator;

namespace EvaluatorTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Evaluator.Evaluate("1 +       A12 - 2", null);
            Console.WriteLine("Done");
        }
    }
}