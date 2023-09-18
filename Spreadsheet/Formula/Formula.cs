// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!
// Last updated: August 2023 (small tweak to API)

using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;

namespace SpreadsheetUtilities;

/// <summary>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision
/// floating-point syntax (without unary preceeding '-' or '+');
/// variables that consist of a letter or underscore followed by
/// zero or more letters, underscores, or digits; parentheses; and the four operator
/// symbols +, -, *, and /.
///
/// Spaces are significant only insofar that they delimit tokenList.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
/// and "x 23" consists of a variable "x" and a number "23".
///
/// Associated with every formula are two delegates: a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form. The validator is used to
/// add extra restrictions on the validity of a variable, beyond the base condition that
/// variables must always be legal: they must consist of a letter or underscore followed
/// by zero or more letters, underscores, or digits.
/// Their use is described in detail in the constructor and method comments.
/// </summary>
public class Formula
{
    private List<string> _formula;
    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.
    /// </summary>
    public Formula(string formula) :
        this(formula, s => s, s => true)
    {
    }

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.
    ///
    /// If the formula contains a variable v such that normalize(v) is not a legal variable,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    ///
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    public Formula(string formula, Func<string, string> normalize, Func<string, bool> isValid)
    {
        // Create a list to store tokens
        List<string> tokenList = new List<string>();

        // Break the input formula into tokens and process them one by one.
        foreach (var token in GetTokens(formula))
        {
            string normalized_token = token;

            if (IsVariable(token))
            {
                // Normalize the token, using the supplied normalizer.

                normalized_token = normalize(token);


                // Check that the token is valid, using the supplied validator.
                if (!isValid(normalized_token))
                {
                    throw new FormulaFormatException(String.Format("Invalid variable '{0}'.", normalized_token));
                }
            }

            // Add the token to the token list.
            tokenList.Add(normalized_token);
        }


        int openParenthesesCount = 0;
        int closeParenthesesCount = 0;

        if (tokenList.Count == 0)
        {
            throw new FormulaFormatException("Cannot have an empty formula");
        }
        else if (!IsNumber(tokenList[0]) && !IsVariable(tokenList[0]) && !tokenList[0].Equals("("))
        {
            throw new FormulaFormatException("Invalid first token, must be number, variable, or opening parenthesis");
        }
        else if (!IsNumber(tokenList[tokenList.Count - 1]) && !IsVariable(tokenList[tokenList.Count - 1]) && !tokenList[tokenList.Count - 1].Equals(")"))
        {
            throw new FormulaFormatException("Invalid last token, must be number, variable, or closing parenthesis");
        }

        // first and last item have already been checked, check the rest against the rules
        for (int i = 0; i < tokenList.Count - 1; i++)
        {
            string token = tokenList[i];

            if (IsOperator(token) || IsVariable(token) || IsNumber(token))
            {
                if (token == "(")
                {
                    openParenthesesCount++;

                    // Check for following valid token
                    if (!(IsNumber(tokenList[i + 1]) || IsVariable(tokenList[i + 1]) || tokenList[i + 1] == "("))
                        throw new FormulaFormatException("Invalid token following '('.");



                }
                else if (token == ")")
                {
                    closeParenthesesCount++;
                    if (!IsOperator(tokenList[i + 1]) && !tokenList[i + 1].Equals(")"))
                    {
                        throw new FormulaFormatException("Any token that immediately follows a number, a variable, " +
                                                            "or a closing parenthesis must be either an operator or " +
                                                            "a closing parenthesis");

                    }
                }
                // Extra Following rule check.
                // If its a number, variable, or close paren. the next thing must be an operator or close paren.
                else if (IsNumber(token) || IsVariable(token) || token.Equals(")"))
                {
                    if (!IsOperator(tokenList[i + 1]) && !tokenList[i + 1].Equals(")"))
                    {
                        throw new FormulaFormatException("Any token that immediately follows a number, a variable, " +
                                                            "or a closing parenthesis must be either an operator or " +
                                                            "a closing parenthesis");

                    }
                }
                // any token following an opening parenthesis or an operator must be either
                // a number, a variable, or an opening parenthesis
                else if (token.Equals("(") || IsOperator(token))
                {
                    if (!IsNumber(tokenList[i + 1]) && !IsVariable(tokenList[i + 1]) && !tokenList[i + 1].Equals("("))
                    {
                        throw new FormulaFormatException("Any token following an opening parenthesis or an operator must" +
                            " be either a number, a variable, or an opening parenthesis");
                    }

                }
            }
            // had an else here but it was dead code
            // all invalid items are caught by the look ahead test
            // that checks if the item following a value/variable is an operator
        }

        // Account for edge case where extra parenthesis is last token
        if (tokenList[tokenList.Count - 1] == ")")
        {
            closeParenthesesCount++;
        }

        if (openParenthesesCount != closeParenthesesCount)
        {
            throw new FormulaFormatException("Open parenthesis count != close parenthesis count. Extra or missing parenthesis");
        }

        _formula = tokenList;


    }

    private static bool IsNumber(string token)
    {
        return double.TryParse(token, out _);
    }

    private static bool IsOperator(string token)
    {
        return token == "+" || token == "-" || token == "*" || token == "/" || token == "(" || token == ")";
    }

    /// <summary>
    /// Helper method do decide if the input string is a valid variable
    /// </summary>
    /// <param name="str">input string that is being validated for proper variable format</param> 
    /// <returns>bool whether the string is a valid variable format</returns> 
    private static bool IsVariable(string str)
    {

        return Regex.IsMatch(str, @"^[a-zA-Z_][a-zA-Z0-9_]*$");

    }


    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to
    /// the constructor.)
    ///
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters
    /// in a string to upper case:
    ///
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    ///
    /// Given a variable symbol as its parameter, lookup returns the variable's value
    /// (if it has one) or throws an ArgumentException (otherwise).
    ///
    /// If no undefined variables or divisions by zero are encountered when evaluating
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    public object Evaluate(Func<string, double> lookup)
    {
        // Create a list to store tokens.
        List<string> tokens = new List<string>();

        // create stacks needed for operations
        Stack<double> valueStack = new Stack<double>();
        Stack<char> action = new Stack<char>();

        // Evaluate all variables using lookup().
        foreach (var token in _formula)
        {
            if (IsVariable(token))
            {
                // Evaluate variables.
                try
                {
                    var value = lookup(token);
                    tokens.Add(value.ToString());
                }
                catch (ArgumentException)
                {
                    // Return FormulaError when lookup() fails to find a variable.
                    return new FormulaError(String.Format("Undefined variable '{0}'.", token));
                }
            }
            else
            {
                // Use numbers and operators as is.
                tokens.Add(token);
            }
        }

        // go through every token
        foreach (string tok in tokens)
        {

            // if token is a number, go in
            if (IsNumber(tok))
            {
                // if there is an operator in the action stack, check if its multiply or divide
                if (action.TryPeek(out char tempOperator) && tempOperator == '*' || tempOperator == '/')
                {
                    try
                    {
                        // there is an operator present, see if its multiply or divide, if so, do that operation
                        valueStack.Push(DoOperation(valueStack.Pop(), double.Parse(tok), action.Pop()));
                    }
                    catch (ArgumentException)
                    {
                        return new FormulaError("Division by zero.");
                    }

                }
                else
                {
                    // if there isn't an operator, push the valueStack into the stack
                    valueStack.Push(double.Parse(tok));
                }
            }
            else
            {
                // token is DEFINITELY an operator at this point

                char symbol = tok[0];

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
                        }

                        break;
                }
            }
        }
        // Last token has been processed
        // if action stack is empty then result should be the only valueStack in valueStack stack
        // if this is untrue, throw exception
        if (action.Count == 0 && valueStack.Count != 0)
        {

            return valueStack.Pop();

        }

        // If operator stack is not empty
        // Assume there is one operator and two values
        return DoOperation(valueStack.Pop(), valueStack.Pop(), action.Pop());

    }

    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this
    /// formula.  No normalization may appear more than once in the enumeration, even
    /// if it appears more than once in this Formula.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    public IEnumerable<string> GetVariables()
    {

        HashSet<string> result = new HashSet<string>();

        foreach (string i in _formula)
        {
            if (IsVariable(i) && !result.Contains(i))
            {
                result.Add(i);
            }
        }
        return result;

    }



    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    public override string ToString()
    {
        return string.Join("", _formula);
    }

    /// <summary>
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    ///
    /// Two Formulae are considered equal if they consist of the same tokenList in the
    /// same order.  To determine token equality, all tokenList are compared as strings
    /// except for numeric tokenList and variable tokenList.
    /// Numeric tokenList are considered equal if they are equal after being "normalized" by
    /// using C#'s standard conversion from string to double (and optionally back to a string).
    /// Variable tokenList are considered equal if their normalized forms are equal, as
    /// defined by the provided normalizer.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    public override bool Equals(object? obj)
    {

        if (obj == null)
        {
            return false;
        }
        else if (obj.GetType() != this.GetType())
        {
            return false;
        }
        else
        {
            return this.GetHashCode() == obj.GetHashCode();
        }

    }


    /// <summary>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator ==(Formula f1, Formula f2)
    {
        return f1.Equals(f2);
    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !f1.Equals(f2);

    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode()
    {
        System.HashCode hash = new System.HashCode();
        foreach (string token in _formula)
        {
            if (!IsNumber(token))
            {
                hash.Add(token.GetHashCode());
            }
            else
            {
                hash.Add(double.Parse(token.ToString()).GetHashCode());
            }
        }
        return hash.ToHashCode();
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
    private static double DoOperation(double num2, double num1, char op)
    {
        switch (op)
        {
            case '+':
                return num1 + num2;
            case '-':
                return num1 - num2;
            case '*':
                return num1 * num2;
        }

        // it is division at this point, keeps compiler happy to always have a return 
        // outside of switch case
        if (num1 == 0)
        {
            throw new ArgumentException("Division by zero error");
        }
        return num2 / num1;

    }


    /// <summary>
    /// Given an expression, enumerates the tokenList that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a legal variable token;
    /// a double literal; and anything that doesn't match one of those patterns.
    /// There are no empty tokenList, and no token contains white space.
    /// </summary>
    private static IEnumerable<string> GetTokens(string formula)
    {
        // Patterns for individual tokenList
        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

        // Enumerate matching tokenList that don't consist solely of white space.
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                yield return s;
            }
        }

    }
}

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(string message) : base(message)
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(string reason) : this()
    {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}
