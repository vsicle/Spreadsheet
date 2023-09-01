using System;
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
                if(substrings[i].Length != 0)
                    ValidateStr(substrings[i]);

                // check if a variable has been presented.
                // if it has then get its value and put into tokens list

                if (IsVariable(substrings[i]))
                {
                    // call variableEvaluator and put that into substrings instead of the variable
                    substrings[i] = variableEvaluator(substrings[i]).ToString();
                }
            }

            // Implement algorithm using Stacks
            
            // By this point the substrings array should only have numbers and operators
            
            for(int i = 0; i < substrings.Length; i++)
            {
                if (substrings[i].Length != 0)
                {
                    // if token is a number, go in
                    if (char.IsNumber(substrings[i][0]))
                    {
                        // if there is an operator in the action stack, check if its multiply or divide
                        if (action.TryPeek(out char tempOperator) && tempOperator == '*' || tempOperator == '/')
                        {
                            // there is an operator present, see if its multiply or divide, if so, do that operation
                            value.Push(DoOperation(value.Pop(), Int32.Parse(substrings[i]), action.Pop()));

                            // add check for division by zero??


                        }
                        else
                        {
                            // if there isn't an operator, push the value into the stack
                            value.Push(Int32.Parse(substrings[i]));
                        }
                    }
                    else
                    {


                        // token is DEFINITELY an operator at this point

                        char symbol = substrings[i][0];

                        // change to string and add double quotes

                        switch (symbol)
                        {
                            case '+':
                            case '-':
                                // if there is something in top of stack
                                
                                if (action.TryPeek(out char tempOperator) && tempOperator == '+' || tempOperator == '-')
                                {
                                    value.Push(DoOperation(value.Pop(), value.Pop(), action.Pop()));
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
                                        // action.Pop();
                                        // value.Push(value.Pop() + value.Pop());

                                        value.Push(DoOperation(value.Pop(), value.Pop(), action.Pop()));


                                    }
                                    else if (tempOp == '-')
                                    {
                                        // do the subtraction
                                        value.Push(DoOperation(value.Pop(), value.Pop(), action.Pop()));

                                        // check if there is a left parenthesis, throw exception if not

                                    }

                                    if (action.TryPeek(out char temp) && temp == '(')
                                    {
                                        action.Pop();
                                    }
                                    else
                                    {
                                        throw new ArgumentException("Missing ( in expression");
                                    }
                                }

                                if (action.TryPeek(out char tempA))
                                {
                                    if (tempA == '*')
                                    {
                                        value.Push(DoOperation(value.Pop(), value.Pop(), action.Pop()));
                                    }
                                    else if (tempA == '/')
                                    {
                                        value.Push(DoOperation(value.Pop(), value.Pop(), action.Pop()));
                                    }
                                }
/*                                    else
                                    {
                                        throw new ArgumentException("Invalid operator, late catch");
                                    }*/
                                
                                break;
                        }
                    }
                }
            }

            // Last token has been processed

            // if action stack is empty
            if(action.Count == 0)
            {
                int result = value.Pop();
                if (value.Count != 0)
                {
                    throw new InvalidOperationException("Something went wrong");
                }
                else
                {
                    return result;
                }
            }

            // If operator stack is not empty
            return DoOperation(value.Pop(), value.Pop(), action.Pop());


            throw new Exception("Something went wrong, unacounted for case");
            

            
        
        }

        /// <summary>
        ///
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void ValidateStr(string str)
        {
            if (!Regex.IsMatch(str, @"^(?:\d+|[a-zA-Z]+\d+|[*/+\-()]+)$"))
            {
                throw new ArgumentException("Invalid Character or format!!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool IsVariable(string str)
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

        private static int DoOperation(int num2, int num1, char op)
        {
            switch (op)
            {
                case '+':
                    return num1 + num2;
                case '-':
                    return num1 - num2;
                case '*':
                    return num1 * num2;
                case '/':
                    if(num2 == 0)
                    {
                        throw new Exception("Division by zero error");
                    }
                    return num2 / num1;
            }

            throw new Exception("DoOperation has failed, likely recieved invalid operator");
        }
    }
}