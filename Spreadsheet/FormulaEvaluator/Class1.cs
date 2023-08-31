using System.Collections.Generic;
using System.Net.Http.Headers;
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

                if (isVariable(substrings[i]))
                {
                    // call variableEvaluator and put that into substrings instead of the variable
                    substrings[i] = variableEvaluator(substrings[i]).ToString();
                }
            }

            // Implement algorithm using Stacks
            
            // By this point the substrings array should only have numbers and operators
            
            for(int i = 0; i < substrings.Length; i++)
            {
               // if token is a number, go in
                if (char.IsNumber(substrings[i][0]))
                {
                    // if there is an operator in the action stack, check if its multiply or divide
                    if(action.TryPeek(out char tempOperator) && tempOperator == '*' || tempOperator == '/')
                    {
                        // there is an operator present, see if its multiply or divide, if so, do that operation
                        if(tempOperator == '*')
                        {
                            action.Pop();
                            value.Push(value.Pop() * Int32.Parse(substrings[i]));

                        }
                        else if (tempOperator == '/')
                        {
                            action.Pop();
                            value.Push(value.Pop() / Int32.Parse(substrings[i]));
                            // add check for division by zero??
                        }

                    }
                    else
                    {
                        // if there isn't an operator, push the value into the stack
                        value.Push(Int32.Parse(substrings[i]));
                    }
                }
                // token is DEFINITELY an operator at this point

                char symbol = substrings[i][0];
                
                switch(symbol)
                {
                    case '+':
                    case '-':
                        // if there is something in top of stack
                        if(action.TryPeek(out char tempOperator))
                        {
                            if(tempOperator == '+')
                            {
                                action.Pop();
                                value.Push(value.Pop() + value.Pop());
                            }
                            else if (tempOperator == '-')
                            {
                                action.Pop();
                                value.Push(value.Pop() - value.Pop());
                            }
                        }
                        action.Push(symbol);
                        break;
                    case '*':
                    case '/':
                    case '(':
                        action.Push(symbol);
                        break;
                    case ')':

                        // check what is at the top of action stack, proceed accordingly
                        if (action.TryPeek(out char tempOp))
                        {
                            if (tempOp == '+')
                            {
                                // do the addition
                                action.Pop();
                                value.Push(value.Pop() + value.Pop());

                                // check if there is a left parenthesis, throw exception if not
                                if(action.TryPeek(out char temp) && temp == '(')
                                {
                                    action.Pop();
                                }
                                else
                                {
                                    throw new ArgumentException("Missing ( in expression");
                                }
                            }
                            else if (tempOp == '-')
                            {
                                // do the subtraction
                                action.Pop();
                                value.Push(value.Pop() - value.Pop());

                                // check if there is a left parenthesis, throw exception if not
                                if (action.TryPeek(out char temp) && temp == '(')
                                {
                                    action.Pop();
                                }
                                else
                                {
                                    throw new ArgumentException("Missing ( in expression");
                                }
                            }
                            else if (tempOp == '*')
                            {
                                action.Pop();
                                value.Push(value.Pop() * value.Pop());
                            }
                            else if (tempOp == '/')
                            {
                                action.Pop();
                                value.Push(value.Pop() / value.Pop());
                            }
                            else
                            {
                                throw new ArgumentException("Invalid operator, late catch");
                            }
                        }
                        break;
                }
            }

            // Last token has been processed

            // if action stack is empty
            if(action.Count == 0)
            {
                int result = value.Pop();
                if (value.Count != 0)
                {
                    throw new InvalidOperationException("Something went wrong, ");
                }
                else
                {
                    return result;
                }
            }

            // If operator stack is not empty
            char lastOp = action.Pop();
            switch (lastOp)
            {
                case '+':
                    value.Push(value.Pop() + value.Pop());
                    return value.Pop();
                case '-':
                    //var v1 = value.Pop();
                    //var v2 = value.Pop();
                    value.Push(value.Pop() - value.Pop());
                    return value.Pop();
            }

            throw new Exception("Something went wrong, unacounted for case");
            

            
        
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool isVariable(string str)
        {
            bool hasLetter = false;
            foreach(char x in str)
            {
                if (char.IsLetter(x))
                {
                    hasLetter = true;
                }
                else if(char.IsDigit(x) && hasLetter)
                {
                    return true;
                }
            }
            return false;
        }
    }
}