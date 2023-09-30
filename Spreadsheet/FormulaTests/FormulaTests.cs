using SpreadsheetUtilities;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FormulaTester
{
    [TestClass]
    public class FormulaTester
    {

        [TestMethod]
        public void CreatingValidFormula()
        {
            Formula a = new Formula("5+z2-    8 * g5");
            Assert.AreEqual(1, 1);

            Formula b = new Formula("x+y+z-7/(4*2-h)");
            Assert.AreEqual(1, 1);

            Formula c = new Formula("1*2-3+4/5");
            Assert.AreEqual(1, 1);

            Formula d = new Formula("x1+x2-y1*y2/z1+(z2*z3)");
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void ConstructorExceptionsTest()
        {
            // a method that converts all the letters in a string to upper case
            Func<string, string> N = str => str.ToUpper();
            // a method that returns true only if a string consists of one letter followed by one digit
            Func<string, bool> V = str => Regex.IsMatch(str, @"^[a-zA-Z][0-9]$");
            // a super bad normalizer, to trigger exception
            Func<string, string> bad = str => str = "!!!" + str + "!!";

            new Formula("x2+y3", N, V);  // should succeed
            try
            {
                new Formula("x+y3", N, V);  // should throw an exception, since V(N("x")) is false
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }
            try
            {
                new Formula("2x+y3", N, V);  // should throw an exception, since "2x+y3" is syntactically incorrect.
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            try
            {
                new Formula("+y3", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            try
            {
                new Formula("3+", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            try
            {
                new Formula("", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            try
            {
                new Formula("    ", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            //Invalid token following (
            try
            {
                new Formula("2+5*(+3)", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }


            // Number, variable, close parenthesis must be followed by operator or close parenthesis
            try
            {
                new Formula("2 2+4", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            // Number, variable, close parenthesis must be followed by operator or close parenthesis
            try
            {
                new Formula("x2 2+4", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            // Number, variable, close parenthesis must be followed by operator or close parenthesis
            try
            {
                new Formula("(4*5)2", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            // Extra parenthesis check
            try
            {
                new Formula("4+8+(4*5))", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            // Extra parenthesis check
            try
            {
                new Formula("((2+2)))", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            // Extra parenthesis check
            try
            {
                new Formula("((1+3)", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            // any token following an opening parenthesis or an operator must be either
            // a number, a variable, or an opening parenthesis
            try
            {
                new Formula("()+2+5", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            // any token following an opening parenthesis or an operator must be either
            // a number, a variable, or an opening parenthesis
            try
            {
                new Formula("(-7", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            // any token following an opening parenthesis or an operator must be either
            // a number, a variable, or an opening parenthesis
            try
            {
                new Formula("2++5", N, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            // Invalid token in formula
            try
            {
                new Formula("2$5");
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            // poor normalizer, should trigger exception
            try
            {
                new Formula("x2+5", bad, V);
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            // a method that converts all the letters in a string to upper case
            Func<string, string> N = str => str.ToUpper();
            // a method that returns true only if a string consists of one letter followed by one digit
            Func<string, bool> V = str => Regex.IsMatch(str, @"^[a-zA-Z][0-9]$");

            Debug.Assert(new Formula("1+1").GetHashCode() == new Formula("1+1").GetHashCode());
            Debug.Assert(new Formula("123").GetHashCode() != new Formula("abc").GetHashCode());
            Debug.Assert(new Formula("1+2").GetHashCode() != new Formula("2+1").GetHashCode());
            Debug.Assert(new Formula("x1+y2").GetHashCode() != new Formula("X1+Y2").GetHashCode());
            Debug.Assert(new Formula("x1+y2", N, s => true).GetHashCode() == (new Formula("X1  +  Y2").GetHashCode()));  // is true
            Debug.Assert(new Formula("2.0 + x7").GetHashCode() == (new Formula("2.000 + x7")).GetHashCode());  // is true
        }

        //[TestMethod]
        //public void GetVariables()
        //{
        //    // a method that converts all the letters in a string to upper case
        //    Func<string, string> N = str => str.ToUpper();
        //    // a method that returns true only if a string consists of one letter followed by one digit
        //    Func<string, bool> V = str => Regex.IsMatch(str, @"^[a-zA-Z][0-9]$");

        //    Debug.Assert(new Formula("1+1").GetVariables().SequenceEqual(new List<string>()));

        //    Debug.Assert(new Formula("x+y*z", N, s => true).GetVariables().SequenceEqual(new List<string> { "X", "Y", "Z" }));  // should enumerate "X", "Y", and "Z"


        //    Debug.Assert(new Formula("x+X*z", N, s => true).GetVariables().SequenceEqual(new List<string> { "X", "Z" }));  // should enumerate "X" and "Z".
        //    Debug.Assert(new Formula("x+X*z").GetVariables().SequenceEqual(new List<string> { "x", "X", "z" }));  // should enumerate "x", "X", and "z".
        //}

        [TestMethod]
        public void ToStringTest()
        {
            // a method that converts all the letters in a string to upper case
            Func<string, string> N = str => str.ToUpper();
            // a method that returns true only if a string consists of one letter followed by one digit
            Func<string, bool> V = str => Regex.IsMatch(str, @"^[a-zA-Z][0-9]$");

            Debug.Assert(new Formula("x + y", N, s => true).ToString() == "X+Y");  // should return "X+Y"
            Debug.Assert(new Formula("x + Y").ToString() == "x+Y");  // should return "x+Y"
        }


        [TestMethod]
        public void EqualsTest()
        {
            // a method that converts all the letters in a string to upper case
            Func<string, string> N = str => str.ToUpper();
            // a method that returns true only if a string consists of one letter followed by one digit
            Func<string, bool> V = str => Regex.IsMatch(str, @"^[a-zA-Z][0-9]$");

            Debug.Assert(new Formula("x + y", N, s => true).ToString() == "X+Y");  // should return "X+Y"
            Debug.Assert(new Formula("x + Y").ToString() == "x+Y");  // should return "x+Y"
            Debug.Assert(new Formula("1+1").Equals(null) == false);  // the argument must be of type Formula
            Debug.Assert(new Formula("1+1").Equals("test") == false);  // the argument must be of type Formula
            Formula f = new Formula("1+1");
            Debug.Assert(f.Equals(f) == true);  // comparing to self is true
            Debug.Assert(new Formula("1+1").Equals("1+1+1") == false);  // The token list lengths must be the same
            Debug.Assert(new Formula("1+2").Equals("2+1") == false);  // The token order must be the same.
            Debug.Assert(new Formula("123").Equals("abc") == false);  // This should not throw exceptions.
            Debug.Assert(new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) == true);  // is true
            Debug.Assert(new Formula("x1+y2").Equals(new Formula("X1+Y2")) == false);  // is false
            Debug.Assert(new Formula("x1+y2").Equals(new Formula("y2+x1")) == false);  // is false
            Debug.Assert(new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) == true);  // is true
        }

        [TestMethod]
        public void EqualandNotEqualsOperatorOverload()
        {
            // a method that converts all the letters in a string to upper case
            Func<string, string> N = str => str.ToUpper();
            // a method that returns true only if a string consists of one letter followed by one digit
            Func<string, bool> V = str => Regex.IsMatch(str, @"^[a-zA-Z][0-9]$");


            Debug.Assert(new Formula("1+1") != new Formula("test"));

            Formula f = new Formula("1+1");
            Debug.Assert(f == f);

            Debug.Assert(new Formula("1+1") != new Formula("1+1+1"));
            Debug.Assert(new Formula("1+2") != new Formula("2+1"));
            Debug.Assert(new Formula("123") != new Formula("abc"));
            Debug.Assert(new Formula("x1+y2", N, s => true) == new Formula("X1  +  Y2"));
            Debug.Assert(new Formula("x1+y2") != new Formula("X1+Y2"));
            Debug.Assert(new Formula("x1+y2") != new Formula("y2+x1"));
            Debug.Assert(new Formula("2.0 + x7") == new Formula("2.000 + x7"));
        }

        [TestMethod]
        public void EvaluateTest()
        {
            // a method that converts all the letters in a string to upper case
            Func<string, string> N = str => str.ToUpper();
            // a method that returns true only if a string consists of one letter followed by one digit
            Func<string, bool> V = str => Regex.IsMatch(str, @"^[a-zA-Z][0-9]$");

            object res;
            res = new Formula("1+1+1").Evaluate(L);
            Debug.Assert((res is double) && ((double)res == 3.0));

            res = new Formula("2+3*5+(3+4*8)*5+2").Evaluate(L);
            Debug.Assert((res is double) && ((double)res == 194.0));

            res = new Formula("1/2").Evaluate(L);
            Debug.Assert((res is double) && ((double)res == 0.5));

            res = new Formula("4-5-3-1.2").Evaluate(L);
            Debug.Assert((res is double) && ((double)res == -5.2));

            res = new Formula("(2-3)-1").Evaluate(L);
            Debug.Assert((res is double) && ((double)res == -2));

            res = new Formula("1+undefined").Evaluate(L);
            Debug.Assert((res is FormulaError) && (((FormulaError)res).Reason == "Undefined variable 'undefined'."));

            res = new Formula("1/0").Evaluate(L);
            Debug.Assert((res is FormulaError) && (((FormulaError)res).Reason == "Division by zero."));

            res = new Formula("x+7", N, s => true).Evaluate(L);
            Debug.Assert((res is double) && ((double)res == 11.0));

            res = new Formula("x+7").Evaluate(L);
            Debug.Assert((res is double) && ((double)res == 9.0));

        }


        static Dictionary<string, double> vars = new Dictionary<string, double>()
    {
        {"x", 2.0},
        {"X", 4.0},
        {"y", 3.0},
        {"c", 100.0},
    };

        static double L(string token)
        {
            try
            {
                return vars[token];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(String.Format("Undefined variable '{0}'.", token));
            }
        }

    }
}