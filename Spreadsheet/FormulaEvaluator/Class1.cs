using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    /// <summary>
    /// Evaluator class used to complete arithmetic using correct order of operations
    /// </summary>
    public static class Evaluator
    {
        // declare the variable lookup delegate
        public delegate int Lookup(String v);
        public static int Evaluate (String expression, Lookup variableEvaluator)
        {
            Stack<int> valueStack = new Stack<int>();
            Stack<char> action = new Stack<char>();

            string[] substrings = Regex.Split(expression, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            // Clean up whitespace and validate all items in the input
            for (int i = 0; i < substrings.Length; i++)
            {
                substrings[i] = substrings[i].Trim();

                // ignore whitespace
                if(substrings[i].Length != 0)
                    ValidateStr(substrings[i]);

                // check if a variable has been presented.
                // if it has then get its valueStack and put into tokens list

                if (IsVariable(substrings[i]))
                {
                    // call variableEvaluator and put that into substrings instead of the variable
                    substrings[i] = variableEvaluator(substrings[i]).ToString();
                    if (substrings[i].Length == 0)
                    {
                        throw new Exception("Variable had no valueStack");
                    }
                }
            }
            
            // go through every token, ignoring whitespace

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
                            valueStack.Push(DoOperation(valueStack.Pop(), Int32.Parse(substrings[i]), action.Pop()));

                        }
                        else
                        {
                            // if there isn't an operator, push the valueStack into the stack
                            valueStack.Push(Int32.Parse(substrings[i]));
                        }
                    }
                    else
                    {
                        // token is DEFINITELY an operator at this point

                        char symbol = substrings[i][0];

                        switch (symbol)
                        {
                            case '+':
                            case '-':
                                // if there is something in top of actions aka operators stack
                                
                                if (action.TryPeek(out char tempOperator) && tempOperator == '+' || tempOperator == '-')
                                {
                                    // do that operation
                                    valueStack.Push(DoOperation(valueStack.Pop(), valueStack.Pop(), action.Pop()));
                                }
                                // push the symbol into action stack
                                action.Push(symbol);
                                break;
                            // all 3 of these do the same thing
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

                                        valueStack.Push(DoOperation(valueStack.Pop(), valueStack.Pop(), action.Pop()));

                                    }
                                    else if (tempOp == '-')
                                    {
                                        // do the subtraction
                                        valueStack.Push(DoOperation(valueStack.Pop(), valueStack.Pop(), action.Pop()));

                                    }

                                    // if the opening parenthesis is found as expected, pop it
                                    // if not, throw exception
                                    if (action.TryPeek(out char temp) && temp == '(')
                                    {
                                        action.Pop();
                                    }
                                    else
                                    {
                                        throw new ArgumentException("Missing ( in expression");
                                    }
                                }

                                // check if theres any multiplication or division, if so do it
                                if (action.TryPeek(out char tempA))
                                {
                                    if (tempA == '*')
                                    {
                                        valueStack.Push(DoOperation(valueStack.Pop(), valueStack.Pop(), action.Pop()));
                                    }
                                    else if (tempA == '/')
                                    {
                                        valueStack.Push(DoOperation(valueStack.Pop(), valueStack.Pop(), action.Pop()));
                                    }
                                }
                                
                                break;
                        }
                    }
                }
            }

            // Last token has been processed
            // if action stack is empty then result should be the only valueStack in valueStack stack
            // if this is untrue, throw exception
            if(action.Count == 0 && valueStack.Count != 0)
            {
                int result = valueStack.Pop();
                if (valueStack.Count != 0)
                {
                    throw new InvalidOperationException("Something went wrong");
                }
                else
                {
                    return result;
                }
            }

            // If operator stack is not empty
            if(valueStack.Count == 2)
            {
                return DoOperation(valueStack.Pop(), valueStack.Pop(), action.Pop());
            }
            else
            {
                throw new ArgumentException("Unary negative or improper input format");
            }
                

        }

        /// <summary>
        /// If the string doesn't match expectations then throw an illegal argument exception
        /// 
        /// </summary>
        /// <param name="str"></param> input string that needs to be validated
        /// <exception cref="ArgumentException"></exception> invalid iput detected
        private static void ValidateStr(string str)
        {
            if (!Regex.IsMatch(str, @"^(?:\d+|[a-zA-Z]+\d+|[*/+\-()]+)$"))
            {
                throw new ArgumentException("Invalid Character or format!!");
            }
        }

        /// <summary>
        /// Helper method do decide if the input string is a valid variable
        /// </summary>
        /// <param name="str"></param> input string that is being validated for proper variable format
        /// <returns></returns> whether the string is a valid variable format
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

        /// <summary>
        /// Helper method that will complete addition, subtraction, multiplication, or division.
        /// Accounting for division by zero
        /// </summary>
        /// <param name="num2"></param> second number in operation
        /// <param name="num1"></param> first number in operation
        /// <param name="op"></param> the operator character
        /// <returns></returns> result of the operation being done
        /// <exception cref="Exception"></exception> thrown for division by zero or invalid operator being passed in (unlikely)
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
                    if(num1 == 0)
                    {
                        throw new ArgumentException("Division by zero error");
                    }
                    return num2 / num1;
            }

            throw new Exception("DoOperation has failed, likely recieved invalid operator");
        }
    }
}