using FormulaEvaluator;

namespace EvaluatorTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine(Evaluator.Evaluate("4/2 + 4 - 2 * 5", null)); // incorrect answer?
            // parenthesis break it
        }
    }
}