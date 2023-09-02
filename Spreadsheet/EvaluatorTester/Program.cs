using FormulaEvaluator;

namespace EvaluatorTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine(Evaluator.Evaluate("2+2*(5*5*5)/3", null)); 

        }
    }
}