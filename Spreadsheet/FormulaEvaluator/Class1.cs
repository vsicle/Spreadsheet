using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    public static class Evaluator
    {
        public delegate int Lookup(String v);
        public static int Evaluate (String expression, Lookup variableEvaluator)
        {
            Stack<int> value = new Stack<int>();
            Stack<char> action = new Stack<char>();

            string[] substrings = Regex.Split(expression, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            // Clean up whitespace and validate all items in the input
            for (int i = 0; i < substrings.Length; i++)
            {
                substrings[i] = substrings[i].Trim();
                ValidateStr(substrings[i]);

                // check if a variable has been presented.
                // if it has then get its value and put into tokens list

                // WRITE HELPER METHOD THAT CHECKS IF SUBSTRING IS A VARIABLE

                /*if (!char.IsLetter(substrings[i]))
                {
                    substrings[i] = variableEvaluator(substrings[i]);
                }
                tokens[i] = substrings[i][0];*/
            }

            // Implement algorithm using Stacks
            
            for(int i = 0; i < substrings.Length; i++)
            {
                // if Token is a NUMBER, go in
                //x = "235"
                // 2
                // A2
                // *
               
                if (char.IsNumber(substrings[i]))
                {
                    // if there is an operator in the action stack, check if its multiply or divide
                    if(action.TryPeek(out char tempOperator) && tempOperator == '*' || tempOperator == '/')
                    {
                        // there is an operator present, see if its multiply or divide, if so, do that operation
                        if(tempOperator == '*')
                        {
                            action.Pop();
                            value.Push(value.Pop() * tokens[i]);

                        }
                        else if (tempOperator == '/')
                        {
                            action.Pop();
                            value.Push(value.Pop() / tokens[i]);
                            // add check for division by zero??
                        }

                    }
                    else
                    {
                        value.Push(tokens[i]);
                    }
                }
                // if token is a VARIABLE, go in
                else if ()
                {

                }
            }
            

            

            return 0;
        
        }

        /// <summary>
        /// 
        /// SHOULD BE GOOD, HAVE TA LOOK AT IT
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void ValidateStr(string str)
        {

            string validOperators = "+-*/()";

            if(str.Length == 1 && validOperators.Contains(str))
            {
                return;
            }

            if (!char.IsNumber(str[str.Length - 1]))
            {
                // put a comment inside of these
                throw new ArgumentException();
            }

            bool letterExpected = true;
            bool canBeDigit = false;

            if (char.IsNumber(str[0]))
            {
                letterExpected = false;
                canBeDigit = true;
            }

            

            foreach (char i in str)
            {
                if (char.IsLetter(i) && letterExpected)
                {
                    canBeDigit = true;
                }
/*                else if (canBeDigit)
                {
                    letterExpected = false;
                }*/
                else if (char.IsDigit(i) && canBeDigit)
                {
                    letterExpected = false;
                }
                else
                {
                    throw new ArgumentException();
                }

            }

            return;
        }

    }
}