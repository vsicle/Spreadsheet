﻿// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!
// Last updated: August 2023 (small tweak to API)

using System.Linq.Expressions;
using System.Text.RegularExpressions;

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
        List<string> tokenList = GetTokens(formula).ToList();


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
        for (int i = 1; i < tokenList.Count - 1; i++)
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
                }
                // Extra Following rule check.
                // If its a number, variable, or close paren. the next thing must be an operator or close paren.
                else if (IsNumber(token) || IsVariable(token) || token.Equals(")"))
                {
                    if (!IsOperator(tokenList[i + 1]) || !tokenList[i + 1].Equals(")"))
                    {
                        throw new FormulaFormatException("Any token that immediately follows a number, a variable, " +
                                                            "or a closing parenthesis must be either an operator or " +
                                                            "a closing parenthesis");

                    }
                }
                //TODO Add if statement for operator following rule
                // any token following an opening parenthesis or an operator must be either
                // a number, a variable, or an opening parenthesis
                else if (token.Equals("(") || IsOperator(token))
                {
                    if (!IsNumber(tokenList[i + 1]) || !IsVariable(tokenList[i + 1]) || tokenList[i + 1].Equals("("))
                    {
                        throw new FormulaFormatException("Any token following an opening parenthesis or an operator must" +
                            " be either a number, a variable, or an opening parenthesis");
                    }

                }
            }
            else
            {
                throw new FormulaFormatException("Invalid token in formula.");
            }
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
        return token == "+" || token == "-" || token == "*" || token == "/";
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
        return "";
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
            if (IsOperator(i) && !result.Contains(i))
            {
                result.Add(i);
            }
        }
        return new List<string>();

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
        if (f1.Equals(f2)) return true;
        else return false;
    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2)
    {
        if (!f1.Equals(f2)) return true;
        else return false;
    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode()
    {
        string noVariableFormula = "";

        // go through the formula and make a copy that doesn't have any variables
        // when there are no variables we isolate the essence of the formula that doesn't change
        // then we call getHashCode 
        foreach (string i in _formula)
        {
            if (!IsVariable(i))
            {
                noVariableFormula = "" + noVariableFormula + i;
            }
        }

        //TODO: does this make an infinite recursive loop? Why or why not
        return noVariableFormula.GetHashCode();
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