using FormulaEvaluator;

namespace EvaluatorTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine(Evaluator.Evaluate(" 3    * 4+2 -5", null)); 
            // incorrect order of operations?
            // parenthesis break it
        }
    }
}